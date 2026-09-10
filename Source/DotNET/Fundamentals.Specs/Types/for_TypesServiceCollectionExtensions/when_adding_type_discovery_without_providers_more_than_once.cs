// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_TypesServiceCollectionExtensions;

public class when_adding_type_discovery_without_providers_more_than_once : Specification
{
    ITypes _afterTheLateProvider;
    ITypes _first;
    ITypes _second;

    void Because()
    {
        // Resolve once first, so a universe for the current provider set is certainly cached before the
        // registration below - that is the state a stale one would be served from.
        _ = Resolve();

        GeneratedTypeDiscoveryRegistry.Register(new a_late_provider());

        _afterTheLateProvider = Resolve();

        ResolveTwiceUnderAnUnchangedProviderSet();
    }

    [Fact] void should_cover_a_provider_registered_after_the_first_call() => _afterTheLateProvider.All.ShouldContain(a_late_provider.ContributedType);
    [Fact] void should_reuse_the_universe_while_its_providers_are_unchanged() => _second.ShouldBeSame(_first);

    static ITypes Resolve() =>
        new ServiceCollection()
            .AddTypeDiscovery()
            .BuildServiceProvider()
            .GetRequiredService<ITypes>();

    static HashSet<Type> RegisteredProviderTypes() =>
        [.. GeneratedTypeDiscoveryRegistry.Providers.Select(_ => _.GetType())];

    /// <summary>
    /// Resolves the pair the reuse fact compares, retrying until no provider was registered across it.
    /// </summary>
    /// <remarks>
    /// Reuse is conditional on the provider set standing still, and in this process it does not stand
    /// still on its own: spec classes running in parallel call <c language="csharp">AddBindingsByConvention</c>, which
    /// walks the assembly reference closure and runs module constructors, so a provider type can first
    /// appear at any moment. A registration landing between the two calls rebuilds the universe by
    /// design, and asserting on that pair would be asserting on xUnit's scheduling. Bounded rather than
    /// looping forever, because the assertion failing is a better outcome than a spec that hangs.
    /// </remarks>
    void ResolveTwiceUnderAnUnchangedProviderSet()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var providerTypes = RegisteredProviderTypes();

            _first = Resolve();
            _second = Resolve();

            if (providerTypes.SetEquals(RegisteredProviderTypes()))
            {
                return;
            }
        }
    }

    /// <summary>
    /// A provider registered by no other spec, contributing one type that nothing else in the universe
    /// defines.
    /// </summary>
    /// <remarks>
    /// The contributed type has to come from outside this assembly to be evidence of anything: this
    /// assembly's own generated provider already declares every type defined in it, so a marker declared
    /// here would be discovered whether or not the late registration was honored. It is otherwise inert -
    /// no spec asks what implements it - so leaving it in the process-wide universe disturbs nothing.
    /// </remarks>
    sealed class a_late_provider : ICanProvideAssembliesForDiscovery
    {
        public static Type ContributedType => typeof(Uri);

        public IEnumerable<Assembly> Assemblies => [typeof(a_late_provider).Assembly];

        public IEnumerable<Type> DefinedTypes => [ContributedType];

        public void Initialize()
        {
        }
    }
}
