// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_overriding_metric_policy;

public class for_an_advised_histogram : given.a_metric_exporter
{
    void Because()
    {
        using var meter = new Meter("Cratis.Specs.PolicyAdvice");
        Collect(options => options.MetricPolicies["spec.policy_advice"] = new() { Boundaries = [0.5, 1] }, () => meter.CreateHistogram<double>("spec.policy_advice", "s", null, tags: null, advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = [1, 2, 3] }).Record(0.1));
    }

    [Fact] void should_use_policy_boundaries_over_advice() => Metric("spec.policy_advice").Points.Single().Boundaries.ShouldEqual<double>([0.5, 1]);
}
