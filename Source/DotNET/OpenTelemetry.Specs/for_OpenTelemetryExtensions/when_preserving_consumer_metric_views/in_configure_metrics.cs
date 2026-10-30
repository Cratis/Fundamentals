// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_preserving_consumer_metric_views;

public class in_configure_metrics : given.a_metric_exporter
{
    void Because()
    {
        _exporter = new();
        var services = new ServiceCollection();
        services.AddOpenTelemetry().WithCratis(options =>
        {
            options.MetricCardinalityLimit = 2;
            options.MetricPolicies["spec.consumer_view"] = new() { TagKeys = ["outcome"] };
            options.ConfigureMetrics = metrics => metrics
                .AddView("spec.consumer_view", new ExplicitBucketHistogramConfiguration { Boundaries = [0.5, 1] })
                .AddReader(new PeriodicExportingMetricReader(_exporter, int.MaxValue));
        });
        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        using var meter = new Meter("Cratis.Specs.ConsumerView");
        meter.CreateHistogram<double>("spec.consumer_view", "s").Record(0.25, new KeyValuePair<string, object?>("consumer", "kept"));
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    [Fact] void should_export_only_the_consumer_stream() => _exporter.Metrics.Count(metric => metric.Name == "spec.consumer_view").ShouldEqual(1);
    [Fact] void should_keep_consumer_boundaries() => Metric("spec.consumer_view").Points.Single().Boundaries.ShouldEqual<double>([0.5, 1]);
    [Fact] void should_keep_consumer_tags() => Metric("spec.consumer_view").Points.Single().Tags["consumer"].ShouldEqual("kept");
    [Fact] void should_run_the_exporter_callback() => Metric("spec.consumer_view").Points.Single().Sum.ShouldEqual(0.25);
}
