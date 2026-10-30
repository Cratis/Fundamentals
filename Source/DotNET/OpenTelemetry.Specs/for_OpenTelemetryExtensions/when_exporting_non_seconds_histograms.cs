// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_exporting_non_seconds_histograms : given.a_metric_exporter
{
    void Because()
    {
        using var meter = new Meter("Cratis.Specs.Milliseconds");
        Collect(_ => { }, () => meter.CreateHistogram<double>("spec.milliseconds", "ms").Record(100));
    }

    [Fact] void should_keep_sdk_default_boundaries() => Metric("spec.milliseconds").Points.Single().Boundaries.ShouldEqual<double>(SdkBoundaries);
}
