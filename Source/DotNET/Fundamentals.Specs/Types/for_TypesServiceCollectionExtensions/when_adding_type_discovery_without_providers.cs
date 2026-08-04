// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_TypesServiceCollectionExtensions;

public class when_adding_type_discovery_without_providers : Specification
{
    IServiceCollection _services;
    ITypes _types;

    void Establish()
    {
        GeneratedTypeDiscoveryRegistry.Register(new for_GeneratedTypeDiscoveryRegistry.a_provider());
        _services = new ServiceCollection();
    }

    void Because()
    {
        // Defaulting happens in one place, so what a consumer resolves from the container reports the
        // mode that was actually chosen - not the caller-supplied one it would report if the container
        // re-derived the provider set for itself.
        _services.AddTypeDiscovery();
        _types = _services.BuildServiceProvider().GetRequiredService<ITypes>();
    }

    [Fact] void should_report_the_mode_that_was_chosen_for_it() => _types.DiscoveryMode.ShouldEqual(TypeDiscoveryMode.Generated);
    [Fact] void should_not_report_it_as_caller_supplied() => _types.DiscoveryMode.ShouldNotEqual(TypeDiscoveryMode.Explicit);
}
