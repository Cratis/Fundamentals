// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_exporting_seconds_histograms;

public class from_an_application_meter : given.a_metric_exporter
{
    void Because()
    {
        using var meter = new Meter("MyApplication.Specs");
        Collect(
            _ => { },
            () =>
            {
                meter.CreateHistogram<double>("spec.application", "s").Record(0.1);
                meter.CreateHistogram<double>("spec.product_view", "s").Record(0.1);
                var counter = meter.CreateCounter<int>("spec.sdk_limit");
                for (var value = 0; value < 2002; value++) counter.Add(1, new KeyValuePair<string, object?>("key", value));
            },
            metrics => metrics.AddMeter("MyApplication.Specs").AddView("spec.product_view", new ExplicitBucketHistogramConfiguration { Boundaries = [0.5, 1] }));
    }

    [Fact] void should_keep_sdk_default_boundaries() => Metric("spec.application").Points.Single().Boundaries.ShouldEqual<double>(SdkBoundaries);
    [Fact] void should_preserve_an_application_view() => Metric("spec.product_view").Points.Single().Boundaries.ShouldEqual<double>([0.5, 1]);
    [Fact] void should_not_duplicate_an_unshaped_application_view() => _exporter.Metrics.Count(metric => metric.Name == "spec.product_view").ShouldEqual(1);
    [Fact] void should_keep_the_sdk_limit() => Metric("spec.sdk_limit").Points.Count(point => point.Tags.ContainsKey("key")).ShouldEqual(2000);
    [Fact] void should_keep_sdk_overflow() => Metric("spec.sdk_limit").Points.Single(point => point.Tags.ContainsKey("otel.metric.overflow")).Sum.ShouldEqual(2);
    [Fact] void should_export_one_stream() => _exporter.Metrics.Count(metric => metric.Name == "spec.application").ShouldEqual(1);
}
