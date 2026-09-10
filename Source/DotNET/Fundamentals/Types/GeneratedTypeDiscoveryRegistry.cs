// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cratis.Types;

/// <summary>
/// Holds compile-time generated type discovery providers registered by consuming assemblies.
/// </summary>
public static class GeneratedTypeDiscoveryRegistry
{
    // Two locks, and the order between them is load-bearing. _gate guards _providers and
    // _closureWalkGate guards _assembliesProcessedByClosureWalk; the walk holds _closureWalkGate across
    // module constructors that call Register, so _closureWalkGate is taken before _gate and never the
    // reverse. One lock for both would hold the provider lock across arbitrary module-initializer code.
#if NET9_0_OR_GREATER
    static readonly Lock _gate = new();
    static readonly Lock _closureWalkGate = new();
#else
    static readonly object _gate = new();
    static readonly object _closureWalkGate = new();
#endif
    static readonly List<ICanProvideAssembliesForDiscovery> _providers = [];
    static readonly HashSet<Assembly> _assembliesProcessedByClosureWalk = [];

    /// <summary>
    /// Gets the currently registered providers.
    /// </summary>
    public static IEnumerable<ICanProvideAssembliesForDiscovery> Providers
    {
        get
        {
            lock (_gate)
            {
                return [.. _providers];
            }
        }
    }

    /// <summary>
    /// Registers a generated provider.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    public static void Register(ICanProvideAssembliesForDiscovery provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        lock (_gate)
        {
            if (_providers.Exists(_ => _.GetType() == provider.GetType()))
            {
                return;
            }

            _providers.Add(provider);
        }
    }

    /// <summary>
    /// Ensures every generated provider reachable through the assembly reference closure has been registered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A generated provider registers itself from a <c language="csharp">[ModuleInitializer]</c> in the assembly that
    /// declares it, and a module constructor runs only when the runtime first needs that assembly -
    /// which for a referenced assembly nothing has touched yet is never.
    /// <see cref="Assembly.Load(AssemblyName)"/> does not run one either. So this walks the reference
    /// closure from the loaded assemblies and the entry assembly, loads what is missing, and runs each
    /// module constructor explicitly. An assembly that cannot be loaded is skipped rather than thrown
    /// on: the closure of a real application names assemblies that are genuinely absent at runtime. A
    /// module initializer that itself throws is not swallowed - it propagates, and because the runtime
    /// caches a failed type initialization, every later call throws the same way.
    /// </para>
    /// <para>
    /// Safe to call repeatedly and from several threads at once. Module constructors run once per
    /// process and <see cref="Register"/> deduplicates by provider type, so a repeated call cannot
    /// double-register; a call that finds no assembly the previous walk had not already processed
    /// returns without walking at all. An assembly loaded after - or concurrently with - a walk is not
    /// in that set, so the next call walks again and picks it up.
    /// </para>
    /// <para>
    /// <c language="csharp">AddBindingsByConvention</c> and <c language="csharp">AddSelfBindings</c> call this for themselves. A host that
    /// builds a type universe <em>before</em> either of those - resolving <see cref="ITypes"/> from a
    /// container, or reading <see cref="Types.Instance"/> - has to call this first, or the universe is
    /// built from an incomplete provider set and silently misses everything the walk would have brought
    /// in. It does not announce itself: a shorter discovery result is indistinguishable from a feature
    /// nobody wrote.
    /// </para>
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Referenced assemblies must be visited to run module initializers that register generated providers.")]
    public static void EnsureProvidersRegistered()
    {
        // The walk only produces a new outcome when an assembly no previous walk has seen enters the
        // AppDomain - module constructors run once per process, so re-walking an unchanged assembly set
        // is pure cost. The gate tracks the exact assemblies a completed walk processed rather than a
        // once-latch or a loaded-assembly count, which keeps the late-load semantics intact under
        // concurrency: an assembly loaded after - or concurrently with - a walk is not yet in the set,
        // so the next registration call walks again and runs its module constructor, while the
        // steady-state call reduces to one set-containment check per loaded assembly.
        lock (_closureWalkGate)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (Array.TrueForAll(loadedAssemblies, _assembliesProcessedByClosureWalk.Contains))
            {
                return;
            }

            WalkAssemblyClosureAndRunModuleConstructors();
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Referenced assemblies must be visited to run module initializers that register generated providers.")]
    static void WalkAssemblyClosureAndRunModuleConstructors()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblies = new HashSet<Assembly>(loadedAssemblies);
        var visitedAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var assemblyNamesToLoad = new Queue<AssemblyName>();

        void EnqueueReferencesFor(Assembly assembly)
        {
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies().Where(_ => visitedAssemblyNames.Add(_.FullName)))
            {
                assemblyNamesToLoad.Enqueue(referencedAssemblyName);
            }
        }

        foreach (var loadedAssembly in loadedAssemblies)
        {
            EnqueueReferencesFor(loadedAssembly);
        }
        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            _ = assemblies.Add(entryAssembly);
            EnqueueReferencesFor(entryAssembly);
        }

        while (assemblyNamesToLoad.TryDequeue(out var assemblyName))
        {
            var assembly = assemblies.SingleOrDefault(_ => AssemblyName.ReferenceMatchesDefinition(_.GetName(), assemblyName));

            if (assembly is null)
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                catch (FileLoadException)
                {
                    continue;
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }

            if (assemblies.Add(assembly))
            {
                EnqueueReferencesFor(assembly);
            }
        }

        foreach (var assembly in assemblies)
        {
            if (!assembly.IsDynamic)
            {
                RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);
            }

            // Dynamic assemblies are marked as processed too - they have no module constructor to run,
            // and leaving them out would make the gate walk again on every call for as long as one is loaded.
            _assembliesProcessedByClosureWalk.Add(assembly);
        }
    }
}
