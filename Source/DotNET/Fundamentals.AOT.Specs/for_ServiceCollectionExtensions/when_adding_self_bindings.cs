// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_ServiceCollectionExtensions;

public class when_adding_self_bindings : Specification
{
    ServiceProvider service_provider;
    IServiceScope scope;
    ScopedSelfBinding result;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddTypeDiscovery();
        services.AddSelfBindings();
        service_provider = services.BuildServiceProvider();
        scope = service_provider.CreateScope();
    }

    void Because() => result = scope.ServiceProvider.GetRequiredService<ScopedSelfBinding>();

    void Destroy()
    {
        scope.Dispose();
        service_provider.Dispose();
    }

    [Fact] void should_resolve_the_scoped_self_binding() => result.ShouldNotBeNull();
}
