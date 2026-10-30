// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_metric_views_are_not_enabled : given.a_metric_exporter
{
    void Because()
    {
        _exporter = new();
        var services = new ServiceCollection();
        services.AddOpenTelemetry().WithCratis(options =>
        {
            options.MetricCardinalityLimit = 2;
            options.SecondsHistogramBoundaries = [0.5, 1];
            options.MetricPolicies["spec.unchanged.seconds"] = new() { TagKeys = ["outcome"], Boundaries = [2, 3], CardinalityLimit = 1 };
        }).WithMetrics(metrics => metrics.AddReader(new PeriodicExportingMetricReader(_exporter, int.MaxValue)));
        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        using var meter = new Meter("Cratis.Specs.UnchangedDefaults");
        meter.CreateHistogram<double>("spec.unchanged.seconds", "s").Record(0.25, new KeyValuePair<string, object?>("identifier", "kept"));
        var counter = meter.CreateCounter<int>("spec.unchanged.counter");
        for (var value = 0; value < 2002; value++) counter.Add(1, new KeyValuePair<string, object?>("key", value));
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    [Fact] void should_keep_sdk_boundaries_without_a_callback() => Metric("spec.unchanged.seconds").Points.Single().Boundaries.ShouldEqual<double>(SdkBoundaries);
    [Fact] void should_keep_all_tags() => Metric("spec.unchanged.seconds").Points.Single().Tags["identifier"].ShouldEqual("kept");
    [Fact] void should_keep_the_sdk_cardinality_limit() => Metric("spec.unchanged.counter").Points.Count(point => point.Tags.ContainsKey("key")).ShouldEqual(2000);
    [Fact] void should_keep_sdk_overflow() => Metric("spec.unchanged.counter").Points.Single(point => point.Tags.ContainsKey("otel.metric.overflow")).Sum.ShouldEqual(2);
    [Fact] void should_export_one_histogram_stream() => _exporter.Metrics.Count(metric => metric.Name == "spec.unchanged.seconds").ShouldEqual(1);
}
