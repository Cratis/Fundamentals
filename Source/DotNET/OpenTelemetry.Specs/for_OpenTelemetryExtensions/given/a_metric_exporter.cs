// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.given;

public class a_metric_exporter : a_clean_environment
{
    protected static readonly double[] SdkBoundaries = [0, 5, 10, 25, 50, 75, 100, 250, 500, 750, 1000, 2500, 5000, 7500, 10000];
    protected metric_exporter _exporter;

    protected void Collect(Action<CratisOpenTelemetryOptions> configure, Action record, Action<MeterProviderBuilder>? configureMetrics = null)
    {
        _exporter = new();
        var services = new ServiceCollection();
        services.AddOpenTelemetry().WithCratis(options =>
        {
            configure(options);
            options.ConfigureMetrics = metrics =>
            {
                configureMetrics?.Invoke(metrics);
                metrics.AddReader(new PeriodicExportingMetricReader(_exporter, int.MaxValue));
            };
        });
        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        record();
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    protected recorded_metric Metric(string name) => _exporter.Metrics.First(metric => metric.Name == name);

    protected record recorded_point(Dictionary<string, object?> Tags, double Sum, IReadOnlyList<double> Boundaries);

    protected record recorded_metric(string Name, string Unit, List<recorded_point> Points);

    protected sealed class metric_exporter : BaseExporter<Metric>
    {
        public List<recorded_metric> Metrics { get; } = [];

        public override ExportResult Export(in Batch<Metric> batch)
        {
            // The SDK reuses points, so retain values rather than Metric instances.
            Metrics.Clear();
            foreach (var metric in batch)
            {
                Metrics.Add(Snapshot(metric));
            }
            return ExportResult.Success;
        }

        static recorded_metric Snapshot(Metric metric)
        {
            List<recorded_point> points = [];
            foreach (ref readonly var point in metric.GetMetricPoints())
            {
                var tags = new Dictionary<string, object?>();
                foreach (var tag in point.Tags)
                {
                    tags.Add(tag.Key, tag.Value);
                }
                var sum = metric.MetricType switch
                {
                    MetricType.Histogram => point.GetHistogramSum(),
                    MetricType.DoubleSum => point.GetSumDouble(),
                    _ => point.GetSumLong()
                };
                var boundaries = metric.MetricType == MetricType.Histogram ? Boundaries(point) : [];
                points.Add(new(tags, sum, boundaries));
            }
            return new(metric.Name, metric.Unit, points);
        }

        static List<double> Boundaries(MetricPoint point)
        {
            List<double> boundaries = [];
            foreach (var bucket in point.GetHistogramBuckets())
            {
                if (double.IsFinite(bucket.ExplicitBound)) boundaries.Add(bucket.ExplicitBound);
            }
            return boundaries;
        }
    }
}
