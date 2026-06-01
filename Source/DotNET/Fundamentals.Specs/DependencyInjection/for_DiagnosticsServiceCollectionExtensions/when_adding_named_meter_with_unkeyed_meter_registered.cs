// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_DiagnosticsServiceCollectionExtensions;

public class when_adding_named_meter_with_unkeyed_meter_registered : Specification
{
    const string meterName = "my-meter";
    IServiceProvider _serviceProvider;
    Metrics.IMeter<when_adding_named_meter_with_unkeyed_meter_registered> _typedMeter;
    Meter _meter;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Meter("unkeyed"));
        services.AddNamedMeter(meterName);
        _serviceProvider = services.BuildServiceProvider();
    }

    void Because()
    {
        _typedMeter = _serviceProvider.GetRequiredKeyedService<Metrics.IMeter<when_adding_named_meter_with_unkeyed_meter_registered>>(meterName);
        _meter = _serviceProvider.GetRequiredKeyedService<Meter>(meterName);
    }

    [Fact] void should_resolve_typed_meter() => _typedMeter.ShouldNotBeNull();
    [Fact] void should_use_the_named_meter() => _typedMeter.ActualMeter.ShouldEqual(_meter);
    [Fact] void should_have_the_correct_name() => _typedMeter.ActualMeter.Name.ShouldEqual(meterName);
}
