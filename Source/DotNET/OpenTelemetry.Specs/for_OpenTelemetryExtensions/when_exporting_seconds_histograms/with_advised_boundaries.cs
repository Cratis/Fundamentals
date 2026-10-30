// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_exporting_seconds_histograms;

public class with_advised_boundaries : given.a_metric_exporter
{
    void Because()
    {
        using var meter = new Meter("Cratis.Specs.Advice");
        Collect(options => options.MetricCardinalityLimit = 100, () => meter.CreateHistogram<double>("spec.advised", "s", null, tags: null, advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = [1, 2, 3] }).Record(0.1));
    }

    [Fact] void should_keep_the_advised_boundaries() => Metric("spec.advised").Points.Single().Boundaries.ShouldEqual<double>([1, 2, 3]);
}
