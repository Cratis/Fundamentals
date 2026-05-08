// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_ServiceCollectionExtensions;

public class when_adding_bindings_by_convention : Specification
{
    ServiceProvider service_provider;
    IConventionalService result;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddTypeDiscovery();
        services.AddBindingsByConvention();
        service_provider = services.BuildServiceProvider();
    }

    void Because() => result = service_provider.GetRequiredService<IConventionalService>();

    void Destroy() => service_provider.Dispose();

    [Fact] void should_resolve_the_conventional_service() => result.ShouldNotBeNull();
    [Fact] void should_resolve_expected_implementation() => result.ShouldBeOfExactType<ConventionalService>();
}
