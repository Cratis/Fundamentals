// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Cratis.DependencyInjection;
using Cratis.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_self_bindings_with_unkeyed_meter_registered : Specification
{
    ServiceProvider _serviceProvider;
    IMeter<when_adding_self_bindings_with_unkeyed_meter_registered> _result;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Meter("unkeyed"));
        services.AddSelfBindings();
        _serviceProvider = services.BuildServiceProvider();
    }

    void Because() => _result = _serviceProvider.GetRequiredService<IMeter<when_adding_self_bindings_with_unkeyed_meter_registered>>();

    void Destroy() => _serviceProvider.Dispose();

    [Fact] void should_resolve_the_meter() => _result.ShouldNotBeNull();
    [Fact] void should_use_the_type_name_for_the_meter() => _result.ActualMeter.Name.ShouldEqual(typeof(when_adding_self_bindings_with_unkeyed_meter_registered).FullName);
}
