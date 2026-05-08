// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_GeneratedTypeDiscoveryProvider;

public class when_registered_via_module_initializer : Specification
{
    ICanProvideConventionsForDependencyInjection[] convention_providers;

    void Because() => convention_providers =
    [
        ..GeneratedTypeDiscoveryRegistry.Providers.OfType<ICanProvideConventionsForDependencyInjection>()
    ];

    [Fact] void should_have_at_least_one_provider() => GeneratedTypeDiscoveryRegistry.Providers.ShouldNotBeEmpty();
    [Fact] void should_expose_conventions_interface() => convention_providers.ShouldNotBeEmpty();
    [Fact] void should_expose_convention_binding_for_conventional_service() =>
        convention_providers
            .SelectMany(_ => _.ConventionServiceBindings)
            .Any(_ => _.ServiceType == typeof(IConventionalService) && _.ImplementationType == typeof(ConventionalService))
            .ShouldBeTrue();
}
