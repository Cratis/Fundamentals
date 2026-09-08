// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry;

public class when_ensuring_providers_are_registered : Specification
{
    const string _probeAssemblyName = "Cratis.Fundamentals.Specs.ModuleInitializerProbe";

    Assembly _probeAssembly;
    Exception _exception;
    int _providersFromTheProbeAssemblyAfterTheFirstCall;
    int _providersFromTheProbeAssemblyAfterTheSecondCall;

    /// <summary>
    /// Loads the probe without registering anything. <see cref="Assembly.Load(string)"/> does not run a
    /// module constructor, so the probe's generated provider is still unregistered by way of this call -
    /// the closure walk is the only thing that runs it.
    /// </summary>
    void Establish() => _probeAssembly = Assembly.Load(_probeAssemblyName);

    void Because()
    {
        // Whether this is the walk that first reaches the probe depends on which spec class ran before
        // it - the registry is process-global and its neighbor in for_ServiceCollectionExtensions walks
        // through AddBindingsByConvention. Scheduling changes how much this observes, never whether it
        // holds: after the call the provider is registered, and that is the contract either way.
        _exception = Catch.Exception(() =>
        {
            GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered();
            _providersFromTheProbeAssemblyAfterTheFirstCall = ProvidersFromTheProbeAssembly();

            GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered();
            _providersFromTheProbeAssemblyAfterTheSecondCall = ProvidersFromTheProbeAssembly();
        });
    }

    [Fact] void should_not_throw_when_called_repeatedly() => _exception.ShouldBeNull();
    [Fact] void should_register_the_provider_the_walk_reaches() => _providersFromTheProbeAssemblyAfterTheFirstCall.ShouldNotEqual(0);
    [Fact] void should_register_nothing_further_on_a_repeated_call() => _providersFromTheProbeAssemblyAfterTheSecondCall.ShouldEqual(_providersFromTheProbeAssemblyAfterTheFirstCall);

    /// <summary>
    /// Counted per assembly rather than over the whole registry, which spec classes running in parallel
    /// add to at any moment. Nothing but the walk registers a provider declared in the probe.
    /// </summary>
    int ProvidersFromTheProbeAssembly() =>
        GeneratedTypeDiscoveryRegistry.Providers.Count(_ => _.GetType().Assembly == _probeAssembly);
}
