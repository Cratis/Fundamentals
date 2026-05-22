// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_self_bindings : Specification
{
    IServiceProvider _serviceProvider;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddSelfBindings();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact] void should_resolve_typed_meter() =>
        _serviceProvider.GetService<Metrics.IMeter<when_adding_self_bindings>>().ShouldNotBeNull();

    [Fact] void should_resolve_typed_activity_source() =>
        _serviceProvider.GetService<Traces.IActivitySource<when_adding_self_bindings>>().ShouldNotBeNull();
}
