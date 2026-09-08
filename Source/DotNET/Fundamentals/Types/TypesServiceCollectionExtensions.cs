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
    /// Gets the <see cref="ITypes"/> universe <see cref="AddTypeDiscovery"/> registers when it is called
    /// without providers.
    /// </summary>
    /// <returns>The current default <see cref="ITypes"/> universe.</returns>
    /// <remarks>
    /// <para>
    /// The same object, not an equivalent one: a caller that needs to hold the universe its container
    /// will resolve can take it from here instead of building a second one or reading it back out of a
    /// service descriptor. Both are the arrangement this exists to remove - a second universe silently
    /// diverges from the container's, and descriptor probing depends on registration order and on the
    /// instance being an <c>ImplementationInstance</c> rather than a factory.
    /// </para>
    /// <para>
    /// It runs <see cref="GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered"/> itself before it
    /// reads the registry, so the universe it returns - and the one the container gets - covers every
    /// generated provider the assembly closure walk can reach, no matter what has or has not run before.
    /// </para>
    /// <para>
    /// <see cref="AddTypeDiscovery"/> keeps returning this same instance for as long as the registered
    /// provider set is unchanged. When a provider is registered afterwards, both this and the next
    /// container's <see cref="ITypes"/> move to the rebuilt universe together; an instance obtained
    /// earlier is the older one and stays that way.
    /// </para>
    /// <para>
    /// A method rather than a property because it is not always a cheap read: when the provider set has
    /// changed since the last call it builds the universe, which is the expensive part of type discovery
    /// described on <see cref="AddTypeDiscovery"/>. A call under an unchanged set returns the built one.
    /// </para>
    /// </remarks>
    public static ITypes CurrentTypeUniverse() => DefaultUniverse();

    /// <summary>
    /// Gets the universe the parameterless <see cref="Types"/> constructor would produce right now,
    /// reusing the previous one when nothing it is built from has changed.
    /// </summary>
    /// <returns>The default universe.</returns>
    /// <remarks>
    /// <para>
    /// Runs the assembly closure walk first, so the default universe is never built from a provider set
    /// the walk had not completed yet. Keyed on the registered generated provider types rather than
    /// cached outright all the same, because the registry is not stable across calls: an assembly loaded
    /// after a walk brings its providers in on the next one, and a universe captured before that is
    /// missing everything it brought in. Handing that to later containers would silently shrink what
    /// they can discover.
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
    /// container from then on shares one universe. Append-only is also what makes the comparison sound
    /// rather than a sampling heuristic: equal sets at two points imply the set was equal throughout.
    /// </para>
    /// <para>
    /// Keying on the generated providers alone is enough. The alternative
    /// <see cref="ProjectReferencedAssemblies"/> and <see cref="PackageReferencedAssemblies"/> fallback
    /// is a pair of singletons that latch on first initialization, so rebuilding from them would read
    /// identical content anyway - and that fallback is unreachable for as long as this package ships a
    /// generated provider for itself.
    /// </para>
    /// </remarks>
    static Types DefaultUniverse()
    {
        // Outside the gate: the walk holds its own lock across module constructors, and a module
        // constructor is free to ask for this universe. Nesting the two locks would let that deadlock.
        GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered();

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
