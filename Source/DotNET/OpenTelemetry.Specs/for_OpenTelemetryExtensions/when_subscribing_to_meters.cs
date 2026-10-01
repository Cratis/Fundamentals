// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_subscribing_to_meters : given.a_clean_environment
{
    readonly metric_exporter _exporter = new();

    void Because()
    {
        var services = new ServiceCollection();
        services.AddOpenTelemetry().WithCratis(options => options.ConfigureMetrics = metrics => metrics.AddReader(new PeriodicExportingMetricReader(_exporter, int.MaxValue)));
        using var provider = services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        using var cratis = new Meter("Cratis.Future.Product", "1.2.3");
        using var application = new Meter("MyApplication.RequestHandler");
        cratis.CreateCounter<int>("cratis.future.count").Add(1);
        application.CreateCounter<int>("application.count").Add(1);
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    [Fact] void should_subscribe_to_future_cratis_meters() => _exporter.Names.ShouldContain("cratis.future.count");
    [Fact] void should_not_subscribe_to_unkeyed_application_meters() => _exporter.Names.ShouldNotContain("application.count");

    sealed class metric_exporter : BaseExporter<Metric>
    {
        public List<string> Names { get; } = [];

        public override ExportResult Export(in Batch<Metric> batch)
        {
            foreach (var metric in batch)
            {
                Names.Add(metric.Name);
            }

            return ExportResult.Success;
        }
    }
}
