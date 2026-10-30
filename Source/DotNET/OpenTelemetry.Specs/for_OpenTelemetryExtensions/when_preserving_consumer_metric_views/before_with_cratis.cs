// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_preserving_consumer_metric_views;

public class before_with_cratis : given.a_metric_exporter
{
    void Because()
    {
        _exporter = new();
        var services = new ServiceCollection();
        var builder = services.AddOpenTelemetry();
        builder.WithMetrics(metrics => metrics
            .AddView("spec.before_view", new ExplicitBucketHistogramConfiguration { Boundaries = [0.5, 1] })
            .AddReader(new PeriodicExportingMetricReader(_exporter, int.MaxValue)));
        builder.WithCratis();
        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        using var meter = new Meter("Cratis.Specs.BeforeView");
        meter.CreateHistogram<double>("spec.before_view", "s").Record(0.25);
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    [Fact] void should_export_only_the_consumer_stream() => _exporter.Metrics.Count(metric => metric.Name == "spec.before_view").ShouldEqual(1);
    [Fact] void should_keep_consumer_boundaries() => Metric("spec.before_view").Points.Single().Boundaries.ShouldEqual<double>([0.5, 1]);
    [Fact] void should_keep_the_measurement() => Metric("spec.before_view").Points.Single().Sum.ShouldEqual(0.25);
}
