// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Types;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/> for setting up type discovery.
/// </summary>
public static class TypesServiceCollectionExtensions
{
#if NET9_0_OR_GREATER
    static readonly Lock _defaultUniverseGate = new();
#else
    static readonly object _defaultUniverseGate = new();
#endif
    static Types? _defaultUniverse;
    static HashSet<Type>? _defaultUniverseProviderTypes;

    /// <summary>
    /// Adds type discovery to the service collection.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <param name="assemblyProviders">Optional collection of <see cref="ICanProvideAssembliesForDiscovery"/>. Will default to <see cref="ProjectReferencedAssemblies"/> and <see cref="PackageReferencedAssemblies"/>.</param>
    /// <returns><see cref="IServiceCollection"/> for continuation.</returns>
    /// <remarks>
    /// Without providers the default universe is built once per distinct set of registered generated
    /// providers and shared by every container after that. Building one is the expensive part of type
    /// discovery: every provider re-initialized, every defined type in every discovered assembly
    /// re-materialized, and a <c>ContractToImplementorsMap</c> fed from all of them. A host that builds
    /// many containers in one process - a spec suite constructing one per scenario - used to pay that in
    /// full per container for a result identical every time.
    /// <para>
    /// Passing providers means asking for a different universe than the default one, so that path is
    /// unaffected and always builds its own instance.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddTypeDiscovery(this IServiceCollection services, IEnumerable<ICanProvideAssembliesForDiscovery>? assemblyProviders = default)
    {
        // Defaulting is left to Types' own parameterless constructor rather than repeated here. Repeating
        // it made every DI-registered ITypes look caller-supplied, which is exactly what the reported
        // discovery mode has to distinguish.
        ITypes types = assemblyProviders is null ? DefaultUniverse() : new Types(assemblyProviders);
        services.AddSingleton<ITypes>(types);
        services
            .AddTransient(typeof(IInstancesOf<>), typeof(InstancesOf<>))
            .AddTransient(typeof(IImplementationsOf<>), typeof(ImplementationsOf<>));
        return services;
    }

    /// <summary>
    /// Gets the universe the parameterless <see cref="Types"/> constructor would produce right now,
    /// reusing the previous one when nothing it is built from has changed.
    /// </summary>
    /// <returns>The default universe.</returns>
    /// <remarks>
    /// <para>
    /// Keyed on the registered generated provider types rather than cached outright, because the
    /// registry is not stable across calls: <c>AddBindingsByConvention</c> and <c>AddSelfBindings</c>
    /// walk the assembly reference closure and run module constructors, so providers appear
    /// <em>after</em> the first container has already registered type discovery. A universe captured
    /// before that walk is missing everything the walk brought in, and handing it to later containers
    /// would silently shrink what they can discover.
    /// </para>
    /// <para>
    /// <see cref="Types.Instance"/> is deliberately not used here for the same reason: it is a static
    /// field, so it snapshots the registry at type initialization - typically before any closure walk -
    /// and can never pick the later providers up.
    /// </para>
    /// <para>
    /// Comparing the provider set keeps the result identical to a fresh construction on every call while
    /// rebuilding only when the inputs actually changed. Registration is append-only and deduplicated by
    /// provider type, so in practice the set stops changing after the first closure walk and every
    /// container from then on shares one universe.
    /// </para>
    /// </remarks>
    static Types DefaultUniverse()
    {
        lock (_defaultUniverseGate)
        {
            // Snapshotted inside the gate and immediately before constructing, so the key describes what
            // the construction below actually read. A provider registered after this point leaves the key
            // understating the universe, which costs one extra rebuild on the next call and never serves
            // a smaller universe than a fresh construction would have.
            var providerTypes = GeneratedTypeDiscoveryRegistry.Providers.Select(_ => _.GetType()).ToHashSet();

            if (_defaultUniverse is null || !_defaultUniverseProviderTypes!.SetEquals(providerTypes))
            {
                _defaultUniverse = new Types();
                _defaultUniverseProviderTypes = providerTypes;
            }

            return _defaultUniverse;
        }
    }
}
