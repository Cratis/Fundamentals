// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_overriding_metric_policy;

public class with_boundaries_and_tags : given.a_metric_exporter
{
    void Because()
    {
        using var meter = new Meter("Cratis.Specs.PolicyTags");
        Collect(
            options =>
            {
                options.MetricPolicies["SPEC.POLICY_TAGS"] = new() { Boundaries = [0.5, 1], TagKeys = ["outcome"] };
                options.MetricPolicies["spec.tagless"] = new() { TagKeys = [] };
            },
            () =>
            {
                var histogram = meter.CreateHistogram<double>("spec.policy_tags", "s");
                histogram.Record(0.1, new("outcome", "success"), new("identifier", "first"));
                histogram.Record(0.2, new("outcome", "success"), new("identifier", "second"));
                var counter = meter.CreateCounter<int>("spec.tagless");
                counter.Add(1, new KeyValuePair<string, object?>("outcome", "success"));
                counter.Add(2, new KeyValuePair<string, object?>("outcome", "failure"));
            });
    }

    [Fact] void should_use_the_policy_boundaries() => Metric("spec.policy_tags").Points.Single().Boundaries.ShouldEqual<double>([0.5, 1]);
    [Fact] void should_keep_only_allowed_tags() => Metric("spec.policy_tags").Points.Single().Tags.Keys.ShouldEqual(["outcome"]);
    [Fact] void should_merge_dropped_tag_series() => Metric("spec.policy_tags").Points.Single().Sum.ShouldBeInRange(0.299, 0.301);
    [Fact] void should_drop_all_tags_for_an_empty_allowlist() => Metric("spec.tagless").Points.Single().Tags.ShouldBeEmpty();
    [Fact] void should_merge_all_series_for_an_empty_allowlist() => Metric("spec.tagless").Points.Single().Sum.ShouldEqual(3);
    [Fact] void should_export_one_stream() => _exporter.Metrics.Count(metric => metric.Name == "spec.policy_tags").ShouldEqual(1);
}
