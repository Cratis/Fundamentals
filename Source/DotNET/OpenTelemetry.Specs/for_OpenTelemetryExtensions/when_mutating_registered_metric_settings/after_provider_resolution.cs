// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_mutating_registered_metric_settings;

public class after_provider_resolution : given.registered_metric_settings
{
    recorded_metric[] _firstExport;

    void Because()
    {
        using var provider = _services.BuildServiceProvider();
        var metrics = provider.GetRequiredService<MeterProvider>();
        using var meter = new Meter("Cratis.Specs.AfterResolution");
        var early = Discover(meter, "early");
        Record(early);
        metrics.ForceFlush(5_000).ShouldBeTrue();
        _firstExport = [.. _exporter.Metrics];
        MutateSettings();
        Record(early);
        Record(Discover(meter, "late"));
        metrics.ForceFlush(5_000).ShouldBeTrue();
    }

    [Fact] void should_export_validated_settings_initially() => ShouldKeepSnapshot(_firstExport, "early", 1);
    [Fact] void should_keep_settings_for_existing_instruments() => ShouldKeepSnapshot(_exporter.Metrics, "early", 2);
    [Fact] void should_use_the_snapshot_for_late_instruments() => ShouldKeepSnapshot(_exporter.Metrics, "late", 1);
    [Fact] void should_export_one_stream_per_instrument() => _exporter.Metrics.Count(metric => metric.Name.StartsWith("spec.", StringComparison.Ordinal)).ShouldEqual(6);
}
