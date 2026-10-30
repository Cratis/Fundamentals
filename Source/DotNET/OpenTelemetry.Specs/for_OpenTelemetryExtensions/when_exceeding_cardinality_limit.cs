// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_exceeding_cardinality_limit : given.a_metric_exporter
{
    void Because()
    {
        using var meter = new Meter("Cratis.Specs.DefaultLimit");
        Collect(options => options.MetricCardinalityLimit = 3, () =>
        {
            var counter = meter.CreateCounter<int>("spec.default_limit");
            for (var value = 0; value < 5; value++) counter.Add(1, new KeyValuePair<string, object?>("key", value));
        });
    }

    [Fact] void should_keep_the_limit_of_tagged_points() => Metric("spec.default_limit").Points.Count(point => point.Tags.ContainsKey("key")).ShouldEqual(3);
    [Fact] void should_tag_the_overflow_point() => Metric("spec.default_limit").Points.Single(point => point.Tags.ContainsKey("otel.metric.overflow")).Tags["otel.metric.overflow"].ShouldEqual(true);
    [Fact] void should_aggregate_overflow_measurements() => Metric("spec.default_limit").Points.Single(point => point.Tags.ContainsKey("otel.metric.overflow")).Sum.ShouldEqual(2);
}
