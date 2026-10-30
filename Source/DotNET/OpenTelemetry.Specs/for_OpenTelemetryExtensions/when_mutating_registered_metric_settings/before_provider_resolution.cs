// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_mutating_registered_metric_settings;

public class before_provider_resolution : given.registered_metric_settings
{
    void Because()
    {
        MutateSettings();
        using var provider = _services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        using var meter = new Meter("Cratis.Specs.BeforeResolution");
        Record(Discover(meter, "early"));
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    [Fact] void should_keep_the_validated_settings() => ShouldKeepSnapshot(_exporter.Metrics, "early", 1);
    [Fact] void should_export_one_stream_per_instrument() => _exporter.Metrics.Count(metric => metric.Name.StartsWith("spec.", StringComparison.Ordinal)).ShouldEqual(3);
}
