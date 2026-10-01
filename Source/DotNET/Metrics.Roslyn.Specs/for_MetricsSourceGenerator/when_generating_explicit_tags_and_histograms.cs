// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_generating_explicit_tags_and_histograms : Specification
{
    readonly List<Instrument> _instruments = [];
    readonly List<KeyValuePair<string, object?>> _tags = [];
    readonly List<double> _measurements = [];
    Activity? _activity;

    void Because()
    {
        var compilation = CompilationFactory.CreateCompilation("""
using System;
using System.Collections.Generic;
using Cratis.Diagnostics;
using Cratis.Metrics;
using Cratis.Traces;

namespace TestApp;

public static partial class Telemetry
{
    [Counter<int>("cratis.test.requests", "Request count", "{request}")]
    public static partial void Count(IMeter<object> meter, [Tag("cratis.test.outcome")] string outcome);

    [Gauge<int>("cratis.test.active", "Active requests", "{request}")]
    public static partial void Gauge(IMeter<object> meter, int value, [Tag("cratis.test.outcome")] string outcome);

    [Histogram<double>("cratis.test.duration", "Request duration", "s")]
    public static partial void Duration(IMeter<object> meter, double seconds, [Tag("cratis.test.outcome")] string outcome);

    [Histogram<double>("cratis.test.scoped.duration", "Scoped duration", "s")]
    public static partial void ScopedDuration(IMeterScope<object> scope, double seconds, [Tag("cratis.test.outcome")] string outcome);

    [Span("cratis.test.request")]
    public static partial IActivityScope<object> Request(IActivitySource<object> source, [Tag("cratis.test.outcome")] string outcome, string legacyTag);
}
""");
        CSharpGeneratorDriver.Create(new MetricsSourceGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var generated, out var diagnostics);
        diagnostics.ShouldBeEmpty();
        using var stream = new MemoryStream();
        var emission = generated.Emit(stream);
        emission.Diagnostics.Where(_ => _.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error).ShouldBeEmpty();
        emission.Success.ShouldBeTrue();
        var telemetry = Assembly.Load(stream.ToArray()).GetType("TestApp.Telemetry")!;
        var typedMeter = new UnkeyedMeter<object>();
        using var meter = typedMeter.ActualMeter;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (ReferenceEquals(instrument.Meter, meter))
            {
                _instruments.Add(instrument);
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<double>((_, measurement, tags, _) =>
        {
            _measurements.Add(measurement);
            _tags.AddRange(tags.ToArray());
        });
        listener.Start();
        using var scope = new MeterScope<object>(typedMeter, new Dictionary<string, object> { ["cratis.test.scope"] = "scoped" });
        telemetry.GetMethod("Count")!.Invoke(null, [typedMeter, "ok"]);
        telemetry.GetMethod("Gauge")!.Invoke(null, [typedMeter, 2, "ok"]);
        telemetry.GetMethod("Duration")!.Invoke(null, [typedMeter, 0.25, "ok"]);
        telemetry.GetMethod("ScopedDuration")!.Invoke(null, [scope, 0.5, "ok"]);
        var source = new Traces.UnkeyedActivitySource<object>();
        using var actualSource = source.ActualSource;
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = candidate => ReferenceEquals(candidate, actualSource),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(activityListener);
        using var activityScope = (Traces.IActivityScope<object>)telemetry.GetMethod("Request")!.Invoke(null, [source, "ok", "legacy"])!;
        _activity = activityScope.Activity;
    }

    [Fact] void should_preserve_units() => _instruments.Select(_ => _.Unit).ShouldContainOnly("{request}", "{request}", "s", "s");
    [Fact] void should_preserve_descriptions() => _instruments.Select(_ => _.Description).ShouldContainOnly("Request count", "Active requests", "Request duration", "Scoped duration");
    [Fact] void should_record_histogram_measurements() => _measurements.ShouldContainOnly(0.25, 0.5);
    [Fact] void should_use_explicit_metric_tags() => _tags.ShouldContain(new("cratis.test.outcome", "ok"));
    [Fact] void should_include_scoped_tags() => _tags.ShouldContain(new("cratis.test.scope", "scoped"));
    [Fact] void should_use_explicit_span_tags() => _activity!.GetTagItem("cratis.test.outcome").ShouldEqual("ok");
    [Fact] void should_preserve_legacy_span_tags() => _activity!.GetTagItem("legacy_tag").ShouldEqual("legacy");
}
