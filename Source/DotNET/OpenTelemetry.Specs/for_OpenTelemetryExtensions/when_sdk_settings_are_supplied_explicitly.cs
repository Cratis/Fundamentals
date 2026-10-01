// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_sdk_settings_are_supplied_explicitly : given.a_clean_environment
{
    ServiceProvider _provider;
    Activity? _activity;
    int? _interval;

    void Because()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_TRACES_SAMPLER"] = "always_off",
            ["OTEL_METRIC_EXPORT_INTERVAL"] = "12345",
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318"
        }).Build();
        var services = new ServiceCollection();
        services.AddOpenTelemetry().WithCratis(configuration);
        _provider = services.BuildServiceProvider();
        _provider.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource("Cratis.Test.Sampler");
        _activity = source.StartActivity("cratis.test.sample");
        _interval = _provider.GetRequiredService<IOptions<PeriodicExportingMetricReaderOptions>>().Value.ExportIntervalMilliseconds;
    }

    void Destroy()
    {
        _activity?.Dispose();
        _provider.Dispose();
    }

    [Fact] void should_not_record_the_activity() => _activity!.IsAllDataRequested.ShouldBeFalse();
    [Fact] void should_not_sample_the_activity() => _activity!.Recorded.ShouldBeFalse();
    [Fact] void should_honor_the_supplied_metric_interval() => _interval.ShouldEqual(12345);
}
