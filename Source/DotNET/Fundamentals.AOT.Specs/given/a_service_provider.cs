// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.given;

public class a_service_provider : Specification
{
    protected ServiceProvider service_provider;

    void Establish()
    {
        // Fail immediately if the source generator did not produce and register a provider.
        // Without this check every spec would silently fall back to reflection discovery and
        // pass even if the generator produced no output at all.
        GeneratedTypeDiscoveryRegistry.Providers.ShouldNotBeEmpty();

        var services = new ServiceCollection();
        services.AddTypeDiscovery();
        services.AddBindingsByConvention();
        services.AddSelfBindings();
        service_provider = services.BuildServiceProvider();
    }

    void Destroy() => service_provider?.Dispose();
}
