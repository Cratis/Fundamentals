// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_overriding_metric_policy;

public class with_a_cardinality_limit : given.a_metric_exporter
{
    void Because()
    {
        using var meter = new Meter("Cratis.Specs.PolicyLimit");
        Collect(
            options =>
            {
                options.MetricCardinalityLimit = 100;
                options.MetricPolicies["spec.policy_limit"] = new() { CardinalityLimit = 2, Boundaries = [0.5, 1] };
            },
            () =>
            {
                var counter = meter.CreateCounter<int>("spec.policy_limit");
                for (var value = 0; value < 4; value++) counter.Add(1, new KeyValuePair<string, object?>("key", value));
            });
    }

    [Fact] void should_keep_the_policy_limit_of_tagged_points() => Metric("spec.policy_limit").Points.Count(point => point.Tags.ContainsKey("key")).ShouldEqual(2);
    [Fact] void should_overflow_after_the_policy_limit() => Metric("spec.policy_limit").Points.Single(point => point.Tags.ContainsKey("otel.metric.overflow")).Sum.ShouldEqual(2);
}
