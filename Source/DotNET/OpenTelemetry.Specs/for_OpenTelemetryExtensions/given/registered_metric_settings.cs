// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.given;

public class registered_metric_settings : a_metric_exporter
{
    protected ServiceCollection _services;
    CratisOpenTelemetryOptions _options;
    IDictionary<string, MetricPolicy> _policies;
    MetricPolicy _policy;
    double[] _seconds;
    double[] _policyBoundaries;
    string[] _tagKeys;

    void Establish()
    {
        _exporter = new();
        _seconds = [0, 0.25, 1];
        _policyBoundaries = [0.5, 1];
        _tagKeys = ["outcome"];
        _policy = new() { Boundaries = _policyBoundaries, TagKeys = _tagKeys, CardinalityLimit = 1 };
        _services = new();
        _services.AddOpenTelemetry().WithCratis(options =>
        {
            _options = options;
            options.EnableMetricViews = true;
            options.MetricCardinalityLimit = 2;
            options.SecondsHistogramBoundaries = _seconds;
            _policies = options.MetricPolicies;
            _policies["SPEC.EARLY.POLICY"] = _policy;
            _policies["SPEC.LATE.POLICY"] = _policy;
            options.ConfigureMetrics = metrics => metrics.AddReader(new PeriodicExportingMetricReader(_exporter, int.MaxValue));
        });
    }

    protected void MutateSettings()
    {
        _options.EnableMetricViews = false;
        _options.ConfigureMetrics = _ => throw new InvalidOperationException("The replacement callback must not run.");
        _options.MetricCardinalityLimit = 100;
        _options.SecondsHistogramBoundaries = [5, 10];
        _seconds[0] = double.NaN;
        _policyBoundaries[0] = double.NaN;
        _tagKeys[0] = " ";
        _policy.Boundaries = [20, 30];
        _policy.TagKeys = ["identifier"];
        _policy.CardinalityLimit = 100;
        _policies.Clear();
        _policies["spec.early.policy"] = new() { Boundaries = [40, 50], TagKeys = ["identifier"], CardinalityLimit = 100 };
        _policies["spec.late.policy"] = _policy;
    }

    protected static instruments Discover(Meter meter, string generation) => new(
        meter.CreateHistogram<double>($"spec.{generation}.policy", "s"),
        meter.CreateHistogram<double>($"spec.{generation}.seconds", "s"),
        meter.CreateCounter<int>($"spec.{generation}.counter"));

    protected static void Record(instruments instruments)
    {
        for (var value = 0; value < 3; value++)
        {
            instruments.Policy.Record(0.5, new KeyValuePair<string, object?>("outcome", value), new KeyValuePair<string, object?>("identifier", "first"));
            instruments.Policy.Record(0.5, new KeyValuePair<string, object?>("outcome", value), new KeyValuePair<string, object?>("identifier", "second"));
            instruments.Seconds.Record(0.25, new KeyValuePair<string, object?>("key", value));
            instruments.Counter.Add(1, new KeyValuePair<string, object?>("key", value));
        }
    }

    protected static recorded_point Overflow(recorded_metric metric) => metric.Points.Single(point => point.Tags.ContainsKey("otel.metric.overflow"));

    protected static IEnumerable<recorded_point> Tagged(recorded_metric metric) => metric.Points.Where(point => !point.Tags.ContainsKey("otel.metric.overflow"));

    protected static void ShouldKeepSnapshot(IEnumerable<recorded_metric> metrics, string generation, int recordings)
    {
        var policy = metrics.Single(metric => metric.Name == $"spec.{generation}.policy");
        policy.Points.ShouldEachConformTo(point => point.Boundaries.SequenceEqual([0.5, 1]));
        Tagged(policy).Single().Tags.Keys.ShouldEqual<string>(["outcome"]);
        Tagged(policy).Single().Sum.ShouldEqual(recordings);
        Overflow(policy).Tags["otel.metric.overflow"].ShouldEqual(true);
        Overflow(policy).Sum.ShouldEqual(2.0 * recordings);
        var seconds = metrics.Single(metric => metric.Name == $"spec.{generation}.seconds");
        seconds.Points.ShouldEachConformTo(point => point.Boundaries.SequenceEqual([0, 0.25, 1]));
        Tagged(seconds).Count().ShouldEqual(2);
        Overflow(seconds).Sum.ShouldEqual(0.25 * recordings);
        var counter = metrics.Single(metric => metric.Name == $"spec.{generation}.counter");
        Tagged(counter).Count().ShouldEqual(2);
        Overflow(counter).Sum.ShouldEqual(recordings);
        counter.Points.Sum(point => point.Sum).ShouldEqual(3.0 * recordings);
    }

    protected record instruments(Histogram<double> Policy, Histogram<double> Seconds, Counter<int> Counter);
}
