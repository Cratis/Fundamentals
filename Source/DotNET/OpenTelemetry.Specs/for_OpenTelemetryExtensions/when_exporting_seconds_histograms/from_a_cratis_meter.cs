// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_exporting_seconds_histograms;

public class from_a_cratis_meter : given.a_metric_exporter
{
    IReadOnlyList<double> _optedOutBoundaries;
    int _disabledMetricCount;

    void Because()
    {
        using var meter = new Meter("Cratis.Specs.Seconds");
        Collect(options => options.SecondsHistogramBoundaries = null, () => meter.CreateHistogram<double>("spec.opted_out", "s").Record(0.1));
        _optedOutBoundaries = Metric("spec.opted_out").Points.Single().Boundaries;
        CollectWhileDisabled(meter);
        _disabledMetricCount = _exporter.Metrics.Count;
        Collect(_ => { }, () => meter.CreateHistogram<double>("spec.seconds", "s").Record(0.1));
    }

    void CollectWhileDisabled(Meter meter)
    {
        Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", "true");
        try
        {
            Collect(_ => { }, () => meter.CreateHistogram<double>("spec.disabled", "s").Record(0.1));
        }
        finally
        {
            Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", null);
        }
    }

    [Fact] void should_keep_sdk_boundaries_when_opted_out() => _optedOutBoundaries.ShouldEqual<double>(SdkBoundaries);
    [Fact] void should_not_export_when_disabled() => _disabledMetricCount.ShouldEqual(0);
    [Fact] void should_use_second_based_boundaries() => Metric("spec.seconds").Points.Single().Boundaries.ShouldEqual<double>(new CratisOpenTelemetryOptions().SecondsHistogramBoundaries!);
    [Fact] void should_export_one_stream() => _exporter.Metrics.Count(metric => metric.Name == "spec.seconds").ShouldEqual(1);
    [Fact] void should_keep_the_unit() => Metric("spec.seconds").Unit.ShouldEqual("s");
}
