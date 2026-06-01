// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cratis.DependencyInjection;
using Cratis.Traces;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_self_bindings_with_unkeyed_activity_source_registered : Specification
{
    ServiceProvider _serviceProvider;
    IActivitySource<when_adding_self_bindings_with_unkeyed_activity_source_registered> _result;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new ActivitySource("unkeyed"));
        services.AddSelfBindings();
        _serviceProvider = services.BuildServiceProvider();
    }

    void Because() => _result = _serviceProvider.GetRequiredService<IActivitySource<when_adding_self_bindings_with_unkeyed_activity_source_registered>>();

    void Destroy() => _serviceProvider.Dispose();

    [Fact] void should_resolve_the_activity_source() => _result.ShouldNotBeNull();
    [Fact] void should_use_the_type_name_for_the_source() => _result.ActualSource.Name.ShouldEqual(typeof(when_adding_self_bindings_with_unkeyed_activity_source_registered).FullName);
}
