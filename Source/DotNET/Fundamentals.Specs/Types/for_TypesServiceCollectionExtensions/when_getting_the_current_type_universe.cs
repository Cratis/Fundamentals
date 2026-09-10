// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_TypesServiceCollectionExtensions;

public class when_getting_the_current_type_universe : Specification
{
    /// <summary>
    /// Captured once per process rather than once per fact.
    /// </summary>
    /// <remarks>
    /// xUnit constructs the spec class once per fact, so <c language="csharp">Because</c> runs once per fact - while the
    /// registration below can only change the provider set the first time, because
    /// <see cref="GeneratedTypeDiscoveryRegistry.Register"/> deduplicates by provider type. A rebuild is
    /// therefore owed exactly once in this process, and capturing per fact would assert one on runs
    /// where nothing new was registered.
    /// </remarks>
    static readonly Lazy<captured_universes> _captured = new(Capture);

    captured_universes _universes;

    void Because() => _universes = _captured.Value;

    [Fact] void should_be_the_instance_the_container_resolves() => _universes.Current.ShouldBeSame(_universes.Registered);
    [Fact] void should_rebuild_when_a_provider_is_registered_afterwards() => _universes.CurrentAfterTheLateProvider.ShouldNotBeSame(_universes.Current);
    [Fact] void should_stay_the_instance_the_container_resolves() => _universes.CurrentAfterTheLateProvider.ShouldBeSame(_universes.RegisteredAfterTheLateProvider);
    [Fact] void should_cover_the_provider_registered_afterwards() => _universes.CurrentAfterTheLateProvider.All.ShouldContain(a_provider_only_this_spec_registers.ContributedType);

    static captured_universes Capture()
    {
        var before = PairUnderAnUnchangedProviderSet();

        GeneratedTypeDiscoveryRegistry.Register(new a_provider_only_this_spec_registers());

        var after = PairUnderAnUnchangedProviderSet();

        return new(before.Current, before.Registered, after.Current, after.Registered);
    }

    static ITypes Resolve() =>
        new ServiceCollection()
            .AddTypeDiscovery()
            .BuildServiceProvider()
            .GetRequiredService<ITypes>();

    static HashSet<Type> RegisteredProviderTypes() =>
        [.. GeneratedTypeDiscoveryRegistry.Providers.Select(_ => _.GetType())];

    /// <summary>
    /// Takes what this accessor returns and what a container resolves, retrying until no provider was
    /// registered across the two.
    /// </summary>
    /// <remarks>
    /// The two are only the same object while the provider set stands still, and in this process it does
    /// not stand still on its own: spec classes running in parallel call <c language="csharp">AddBindingsByConvention</c>,
    /// which walks the assembly reference closure and runs module constructors, so a provider type can
    /// first appear at any moment. A registration landing between the two calls rebuilds the universe by
    /// design, and asserting on that pair would be asserting on xUnit's scheduling. Bounded rather than
    /// looping forever, because the assertion failing is a better outcome than a spec that hangs.
    /// </remarks>
    /// <returns>The pair.</returns>
    static (ITypes Current, ITypes Registered) PairUnderAnUnchangedProviderSet()
    {
        for (var attempt = 0; ; attempt++)
        {
            var providerTypes = RegisteredProviderTypes();
            var pair = (Current: TypesServiceCollectionExtensions.CurrentTypeUniverse(), Registered: Resolve());

            if (attempt == 9 || providerTypes.SetEquals(RegisteredProviderTypes()))
            {
                return pair;
            }
        }
    }

    /// <summary>
    /// The universes taken before and after a provider is registered.
    /// </summary>
    /// <param name="Current">What the accessor returned before the registration.</param>
    /// <param name="Registered">What a container resolved before the registration.</param>
    /// <param name="CurrentAfterTheLateProvider">What the accessor returned after it.</param>
    /// <param name="RegisteredAfterTheLateProvider">What a container resolved after it.</param>
    sealed record captured_universes(
        ITypes Current,
        ITypes Registered,
        ITypes CurrentAfterTheLateProvider,
        ITypes RegisteredAfterTheLateProvider);

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
    sealed class a_provider_only_this_spec_registers : ICanProvideAssembliesForDiscovery
    {
        public static Type ContributedType => typeof(Version);

        public IEnumerable<Assembly> Assemblies => [typeof(a_provider_only_this_spec_registers).Assembly];

        public IEnumerable<Type> DefinedTypes => [ContributedType];

        public void Initialize()
        {
        }
    }
}
