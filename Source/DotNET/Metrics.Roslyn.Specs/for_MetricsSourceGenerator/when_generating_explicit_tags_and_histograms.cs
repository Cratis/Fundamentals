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
    readonly Dictionary<string, KeyValuePair<string, object?>[]> _tags = [];
    readonly List<double> _measurements = [];
    Activity? _activity;

    void Because()
    {
        var compilation = CompilationFactory.CreateCompilation(@"
using System;
using System.Collections.Generic;
using Cratis.Diagnostics;
using Cratis.Metrics;
using Cratis.Traces;

namespace TestApp;

public static partial class Telemetry
{
    [Counter<int>(""cratis.test.requests"", ""Request count"", ""{request}"")]
    public static partial void Count(IMeter<object> meter, [Tag(""cratis.test.outcome"")] string outcome);

    [Gauge<int>(""cratis.test.active"", ""Active requests"", ""{request}"")]
    public static partial void Gauge(IMeter<object> meter, int value, [Tag(""cratis.test.outcome"")] string outcome);

    [Histogram<double>(""cratis.test.duration"", ""Request duration"", ""s"")]
    public static partial void Duration(IMeter<object> meter, double seconds, [Tag(""cratis.test.outcome"")] string outcome);

    [Histogram<double>(""cratis.test.scoped.duration"", ""Scoped duration"", ""s"")]
    public static partial void ScopedDuration(IMeterScope<object> scope, double value, [Tag(""cratis.test.outcome"")] string outcome, string tags, string tags_, string scopeTag, string scopeTag_);

    [Counter<int>(""cratis.test.legacy.requests"", ""Legacy request count"")]
    public static partial void LegacyCount(IMeter<object> meter, [Tag(""cratis.test.outcome"")] string outcome);

    [Gauge<int>(""cratis.test.legacy.active"", ""Legacy active requests"")]
    public static partial void LegacyGauge(IMeter<object> meter, int value, [Tag(""cratis.test.outcome"")] string outcome);

    [Histogram<double>(""cratis.test.unitless"", ""Unitless measurement"")]
    public static partial void Unitless(IMeter<object> meter, double value);

    [Span(""cratis.test.request"")]
    public static partial IActivityScope<object> Request(IActivitySource<object> source, [Tag(""cratis.test.outcome"")] string outcome, string legacyTag);
}
");
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
        listener.SetMeasurementEventCallback<int>((instrument, _, tags, _) => _tags[instrument.Name] = tags.ToArray());
        listener.SetMeasurementEventCallback<double>((instrument, measurement, tags, _) =>
        {
            _measurements.Add(measurement);
            _tags[instrument.Name] = tags.ToArray();
        });
        listener.Start();
        using var scope = new MeterScope<object>(typedMeter, new Dictionary<string, object> { ["cratis.test.scope"] = "scoped" });
        telemetry.GetMethod("Count")!.Invoke(null, [typedMeter, "ok"]);
        telemetry.GetMethod("Gauge")!.Invoke(null, [typedMeter, 2, "ok"]);
        telemetry.GetMethod("Duration")!.Invoke(null, [typedMeter, 0.25, "ok"]);
        telemetry.GetMethod("ScopedDuration")!.Invoke(null, [scope, 0.5, "ok", "tags", "tags_", "scopeTag", "scopeTag_"]);
        telemetry.GetMethod("LegacyCount")!.Invoke(null, [typedMeter, "ok"]);
        telemetry.GetMethod("LegacyGauge")!.Invoke(null, [typedMeter, 3, "ok"]);
        telemetry.GetMethod("Unitless")!.Invoke(null, [typedMeter, 0.75]);
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

    [Fact] void should_preserve_units() => _instruments.Take(4).Select(_ => _.Unit).ShouldContainOnly("{request}", "{request}", "s", "s");
    [Fact] void should_preserve_descriptions() => _instruments.Take(4).Select(_ => _.Description).ShouldContainOnly("Request count", "Active requests", "Request duration", "Scoped duration");
    [Fact] void should_record_histogram_measurements() => _measurements.ShouldContainOnly(0.25, 0.5, 0.75);
    [Fact] void should_use_explicit_counter_tags() => _tags["cratis.test.requests"].ShouldContain(new KeyValuePair<string, object?>("cratis.test.outcome", "ok"));
    [Fact] void should_use_explicit_gauge_tags() => _tags["cratis.test.active"].ShouldContain(new KeyValuePair<string, object?>("cratis.test.outcome", "ok"));
    [Fact] void should_use_explicit_histogram_tags() => _tags["cratis.test.duration"].ShouldContain(new KeyValuePair<string, object?>("cratis.test.outcome", "ok"));
    [Fact] void should_use_explicit_scoped_histogram_tags() => _tags["cratis.test.scoped.duration"].ShouldContain(new KeyValuePair<string, object?>("cratis.test.outcome", "ok"));
    [Fact] void should_include_scoped_tags() => _tags["cratis.test.scoped.duration"].ShouldContain(new KeyValuePair<string, object?>("cratis.test.scope", "scoped"));
    [Fact] void should_preserve_tags_named_like_generated_locals() => _tags["cratis.test.scoped.duration"].ShouldContain(new KeyValuePair<string, object?>("tags", "tags"));
    [Fact] void should_use_explicit_legacy_counter_tags() => _tags["cratis.test.legacy.requests"].ShouldContain(new KeyValuePair<string, object?>("cratis.test.outcome", "ok"));
    [Fact] void should_use_explicit_legacy_gauge_tags() => _tags["cratis.test.legacy.active"].ShouldContain(new KeyValuePair<string, object?>("cratis.test.outcome", "ok"));
    [Fact] void should_leave_legacy_counter_unit_null() => _instruments.Single(_ => _.Name == "cratis.test.legacy.requests").Unit.ShouldBeNull();
    [Fact] void should_leave_legacy_gauge_unit_null() => _instruments.Single(_ => _.Name == "cratis.test.legacy.active").Unit.ShouldBeNull();
    [Fact] void should_leave_histogram_unit_null_when_omitted() => _instruments.Single(_ => _.Name == "cratis.test.unitless").Unit.ShouldBeNull();
    [Fact] void should_use_explicit_span_tags() => _activity!.GetTagItem("cratis.test.outcome").ShouldEqual("ok");
    [Fact] void should_preserve_legacy_span_tags() => _activity!.GetTagItem("legacy_tag").ShouldEqual("legacy");
}
