// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_DiagnosticsServiceCollectionExtensions;

public class when_adding_versioned_diagnostics : Specification
{
    ServiceProvider _provider;
    Metrics.IMeter<object> _meter;
    Traces.IActivitySource<object> _source;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddNamedMeter("Cratis.Test", version: "1.2.3");
        services.AddNamedActivitySource("Cratis.Test", version: "1.2.3");
        _provider = services.BuildServiceProvider();
    }

    void Because()
    {
        _meter = _provider.GetRequiredKeyedService<Metrics.IMeter<object>>("Cratis.Test");
        _source = _provider.GetRequiredKeyedService<Traces.IActivitySource<object>>("Cratis.Test");
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_set_the_meter_version() => _meter.ActualMeter.Version.ShouldEqual("1.2.3");
    [Fact] void should_set_the_source_version() => _source.ActualSource.Version.ShouldEqual("1.2.3");
}
