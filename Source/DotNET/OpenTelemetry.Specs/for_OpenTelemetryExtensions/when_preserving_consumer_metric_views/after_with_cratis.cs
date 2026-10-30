// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_preserving_consumer_metric_views;

public class after_with_cratis : given.a_metric_exporter
{
    void Because()
    {
        _exporter = new();
        var services = new ServiceCollection();
        var builder = services.AddOpenTelemetry().WithCratis(options => options.MetricCardinalityLimit = 2);
        builder.WithMetrics(metrics => metrics
            .AddView("spec.after_view", new ExplicitBucketHistogramConfiguration { Boundaries = [0.5, 1] })
            .AddReader(new PeriodicExportingMetricReader(_exporter, int.MaxValue)));
        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        using var meter = new Meter("Cratis.Specs.AfterView");
        meter.CreateHistogram<double>("spec.after_view", "s").Record(0.25);
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    [Fact] void should_export_only_the_consumer_stream() => _exporter.Metrics.Count(metric => metric.Name == "spec.after_view").ShouldEqual(1);
    [Fact] void should_keep_consumer_boundaries() => Metric("spec.after_view").Points.Single().Boundaries.ShouldEqual<double>([0.5, 1]);
    [Fact] void should_keep_the_measurement() => Metric("spec.after_view").Points.Single().Sum.ShouldEqual(0.25);
}
