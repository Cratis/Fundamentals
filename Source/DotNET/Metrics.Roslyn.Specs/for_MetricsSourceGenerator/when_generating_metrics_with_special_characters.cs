// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Cratis.Traces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_generating_metrics_with_special_characters : Specification
{
    const string Description = "Number of \"requests\" in C:\\metrics\n<all> & 'scoped'\r\t";
    const string SpanName = "requests_\"span\"\\total\n<all> & 'scoped'";
    readonly List<Instrument> _instruments = [];
    ImmutableArray<Diagnostic> _generatorDiagnostics = [];
    EmitResult _emission = null!;
    string _generatedSource = string.Empty;
    string? _activityName;

    void Because()
    {
        var compilation = CompilationFactory.CreateCompilation(@"
using System;
using System.Collections.Generic;
using Cratis.Metrics;
using Cratis.Traces;

namespace TestApp;

public static partial class Metrics
{
    [Counter<int>(""requests_\""count\""\\total"", ""Number of \""requests\"" in C:\\metrics\n<all> & 'scoped'\r\t"")]
    public static partial void Count(IMeter<object> meter);

    [Counter<long>(""scoped_\""count\""\\total"", ""Number of \""requests\"" in C:\\metrics\n<all> & 'scoped'\r\t"")]
    public static partial void CountScoped(IMeterScope<object> scope, long increment);

    [Gauge<double>(""requests_\""gauge\""\\current"", ""Number of \""requests\"" in C:\\metrics\n<all> & 'scoped'\r\t"")]
    public static partial void Record(IMeter<object> meter, double measurement);

    [Gauge<int>(""scoped_\""gauge\""\\current"", ""Number of \""requests\"" in C:\\metrics\n<all> & 'scoped'\r\t"")]
    public static partial void RecordScoped(IMeterScope<object> scope, int measurement);

    [Span(""requests_\""span\""\\total\n<all> & 'scoped'"")]
    public static partial IActivityScope<object> StartSpan(IActivitySource<object> source);
}
");
        var driver = CSharpGeneratorDriver.Create(new MetricsSourceGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out _generatorDiagnostics);
        _generatedSource = driver.GetRunResult().GeneratedTrees.Single().GetText().ToString();

        using var stream = new MemoryStream();
        _emission = generatedCompilation.Emit(stream);
        if (!_emission.Success)
        {
            return;
        }
        var metrics = Assembly.Load(stream.ToArray()).GetType("TestApp.Metrics")!;

        var typedMeter = new UnkeyedMeter<object>();
        using var meter = typedMeter.ActualMeter;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, _) =>
        {
            if (ReferenceEquals(instrument.Meter, meter))
            {
                _instruments.Add(instrument);
            }
        };
        listener.Start();

        using var scope = new MeterScope<object>(typedMeter, new Dictionary<string, object>());
        metrics.GetMethod("Count")!.Invoke(null, [typedMeter]);
        metrics.GetMethod("CountScoped")!.Invoke(null, [scope, 2L]);
        metrics.GetMethod("Record")!.Invoke(null, [typedMeter, 3.0]);
        metrics.GetMethod("RecordScoped")!.Invoke(null, [scope, 4]);

        var typedSource = new UnkeyedActivitySource<object>();
        using var source = typedSource.ActualSource;
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = actualSource => ReferenceEquals(actualSource, source),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(activityListener);
        using var activityScope = (IActivityScope<object>)metrics.GetMethod("StartSpan")!.Invoke(null, [typedSource])!;
        _activityName = activityScope.Activity?.OperationName;
    }

    [Fact] void should_not_report_generator_diagnostics() => _generatorDiagnostics.ShouldBeEmpty();
    [Fact] void should_compile_the_generated_code() => _emission.Success.ShouldBeTrue();
    [Fact] void should_not_report_compilation_warnings_or_errors() => _emission.Diagnostics.Where(_ => _.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error).ShouldBeEmpty();
    [Fact] void should_preserve_the_span_name() => _activityName.ShouldEqual(SpanName);
    [Fact] void should_emit_csharp_string_literals_without_html_escaping() => _generatedSource.ShouldContain("description: \"Number of \\\"requests\\\" in C:\\\\metrics\\n<all> & 'scoped'\\r\\t\"");
    [Fact] void should_publish_all_four_generated_instruments() => _instruments.Count.ShouldEqual(4);
    [Fact] void should_preserve_instrument_names() => _instruments.Select(_ => _.Name).ShouldContainOnly("requests_\"count\"\\total", "scoped_\"count\"\\total", "requests_\"gauge\"\\current", "scoped_\"gauge\"\\current");
    [Fact] void should_leave_all_instrument_units_null() => _instruments.Select(_ => _.Unit).ShouldContainOnly(null, null, null, null);
    [Fact] void should_preserve_all_instrument_descriptions() => _instruments.Select(_ => _.Description).ShouldContainOnly(Description, Description, Description, Description);
}
