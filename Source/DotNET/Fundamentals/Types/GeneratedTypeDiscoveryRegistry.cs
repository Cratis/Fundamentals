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

    /// <summary>
    /// Walks the reference closure of every assembly no previous walk processed, loads what is missing and runs
    /// each newly reached module constructor.
    /// </summary>
    /// <remarks>
    /// The walk is incremental. An assembly a previous walk processed had its whole reference closure loaded
    /// and processed by that walk, so only what is new since then - an assembly loaded by the application, a
    /// late-loaded plugin, a dynamic or collectible assembly a library emitted - is walked from. Starting over
    /// from every loaded assembly on each call made a walk cost the size of the whole closure, and with every
    /// name resolved by asking each loaded assembly for its <see cref="AssemblyName"/> the cost of one walk grew
    /// with the square of it. A process that emits an assembly between calls - a spec harness building a service
    /// provider per spec does - paid that on every call.
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Referenced assemblies must be visited to run module initializers that register generated providers.")]
    static void WalkAssemblyClosureAndRunModuleConstructors()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var loadedByName = new Dictionary<string, List<(AssemblyName Name, Assembly Assembly)>>(StringComparer.OrdinalIgnoreCase);
        foreach (var loadedAssembly in loadedAssemblies)
        {
            AddByName(loadedAssembly);
        }

        var newlyReached = new List<Assembly>();
        var reached = new HashSet<Assembly>();
        var visitedAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var assemblyNamesToLoad = new Queue<AssemblyName>();

        foreach (var loadedAssembly in loadedAssemblies)
        {
            Reach(loadedAssembly);
        }

        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            Reach(entryAssembly);
        }

        while (assemblyNamesToLoad.TryDequeue(out var assemblyName))
        {
            var assembly = Find(assemblyName);

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

                AddByName(assembly);
            }

            Reach(assembly);
        }

        foreach (var assembly in newlyReached)
        {
            if (!assembly.IsDynamic)
            {
                RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);
            }

            // Dynamic assemblies are marked as processed too - they have no module constructor to run,
            // and leaving them out would make the gate walk again on every call for as long as one is loaded.
            _assembliesProcessedByClosureWalk.Add(assembly);
        }

        void Reach(Assembly assembly)
        {
            // A processed assembly's closure was walked when it was processed; walking it again finds nothing.
            if (_assembliesProcessedByClosureWalk.Contains(assembly) || !reached.Add(assembly))
            {
                return;
            }

            newlyReached.Add(assembly);
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                if (visitedAssemblyNames.Add(referencedAssemblyName.FullName))
                {
                    assemblyNamesToLoad.Enqueue(referencedAssemblyName);
                }
            }
        }

        void AddByName(Assembly assembly)
        {
            var name = assembly.GetName();
            if (name.Name is null)
            {
                return;
            }

            if (!loadedByName.TryGetValue(name.Name, out var candidates))
            {
                candidates = [];
                loadedByName[name.Name] = candidates;
            }

            candidates.Add((name, assembly));
        }

        Assembly? Find(AssemblyName reference)
        {
            if (reference.Name is null || !loadedByName.TryGetValue(reference.Name, out var candidates))
            {
                return null;
            }

            foreach (var (name, assembly) in candidates)
            {
                if (AssemblyName.ReferenceMatchesDefinition(name, reference))
                {
                    return assembly;
                }
            }

            return null;
        }
    }
}
