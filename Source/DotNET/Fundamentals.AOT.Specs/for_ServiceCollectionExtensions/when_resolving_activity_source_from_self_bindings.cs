// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;
using Cratis.Traces;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_ServiceCollectionExtensions;

public class when_resolving_activity_source_from_self_bindings : Specification
{
    ServiceProvider _serviceProvider;
    IActivitySource<when_resolving_activity_source_from_self_bindings> _result;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddTypeDiscovery();
        services.AddSelfBindings();
        _serviceProvider = services.BuildServiceProvider();
    }

    void Because() => _result = _serviceProvider.GetRequiredService<IActivitySource<when_resolving_activity_source_from_self_bindings>>();

    void Destroy() => _serviceProvider.Dispose();

    [Fact] void should_resolve_the_activity_source() => _result.ShouldNotBeNull();
}
