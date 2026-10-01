// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_DiagnosticsServiceCollectionExtensions;

public class when_adding_unversioned_diagnostics : Specification
{
    ServiceProvider _provider;
    Metrics.IMeter<object> _meter;
    Traces.IActivitySource<object> _source;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddNamedMeter("Cratis.Test.Unversioned");
        services.AddNamedActivitySource("Cratis.Test.Unversioned");
        _provider = services.BuildServiceProvider();
    }

    void Because()
    {
        _meter = _provider.GetRequiredKeyedService<Metrics.IMeter<object>>("Cratis.Test.Unversioned");
        _source = _provider.GetRequiredKeyedService<Traces.IActivitySource<object>>("Cratis.Test.Unversioned");
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_keep_the_meter_version_null() => _meter.ActualMeter.Version.ShouldBeNull();
    [Fact] void should_keep_the_source_version_empty() => _source.ActualSource.Version.ShouldEqual(string.Empty);
}
