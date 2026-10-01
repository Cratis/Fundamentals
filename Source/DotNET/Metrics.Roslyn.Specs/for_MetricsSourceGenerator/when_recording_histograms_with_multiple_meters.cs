// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_recording_histograms_with_multiple_meters : Specification
{
    readonly List<Instrument> _instruments = [];
    readonly List<(Meter Meter, string Name, double Value)> _measurements = [];
    Meter _firstMeter;
    Meter _secondMeter;
    Meter _sameIdentityMeter;

    void Because()
    {
        var compilation = CompilationFactory.CreateCompilation(@"
using System;
using System.Collections.Generic;
using Cratis.Metrics;

namespace TestApp;

public static partial class Telemetry
{
    [Histogram<double>(""cratis.test.duration"", ""Duration"", ""s"")]
    public static partial void Duration(IMeter<object> meter, double value, string actualMeter, string histogramMeter);

    [Histogram<double>(""cratis.test.scoped.duration"", ""Scoped duration"", ""s"")]
    public static partial void ScopedDuration(IMeterScope<object> scope, double value, string actualMeter, string histogramMeter);
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
        using var firstMeter = new Meter("Cratis.First", "1.0.0");
        using var secondMeter = new Meter("Cratis.Second", "2.0.0");
        using var sameIdentityMeter = new Meter("Cratis.Second", "2.0.0");
        _firstMeter = firstMeter;
        _secondMeter = secondMeter;
        _sameIdentityMeter = sameIdentityMeter;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (ReferenceEquals(instrument.Meter, firstMeter) || ReferenceEquals(instrument.Meter, secondMeter) || ReferenceEquals(instrument.Meter, sameIdentityMeter))
            {
                _instruments.Add(instrument);
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<double>((instrument, value, _, _) => _measurements.Add((instrument.Meter, instrument.Name, value)));
        listener.Start();

        Record(firstMeter, 0.1);
        Record(secondMeter, 0.2);
        firstMeter.Dispose();
        Record(secondMeter, 0.3);
        Record(sameIdentityMeter, 0.4);

        void Record(Meter meter, double value)
        {
            var typedMeter = new Meter<object>(meter);
            using var scope = new MeterScope<object>(typedMeter, new Dictionary<string, object> { ["cratis.test.scope"] = "scoped" });
            telemetry.GetMethod("Duration")!.Invoke(null, [typedMeter, value, "actual", "histogram"]);
            telemetry.GetMethod("ScopedDuration")!.Invoke(null, [scope, value, "actual", "histogram"]);
        }
    }

    [Fact] void should_record_unscoped_measurements_through_the_first_meter() => ValuesFor(_firstMeter, "cratis.test.duration").ShouldContainOnly(0.1);
    [Fact] void should_record_scoped_measurements_through_the_first_meter() => ValuesFor(_firstMeter, "cratis.test.scoped.duration").ShouldContainOnly(0.1);
    [Fact] void should_record_unscoped_measurements_through_the_second_meter_after_disposing_the_first() => ValuesFor(_secondMeter, "cratis.test.duration").ShouldContainOnly(0.2, 0.3);
    [Fact] void should_record_scoped_measurements_through_the_second_meter_after_disposing_the_first() => ValuesFor(_secondMeter, "cratis.test.scoped.duration").ShouldContainOnly(0.2, 0.3);
    [Fact] void should_distinguish_unscoped_meters_with_the_same_name_and_version() => ValuesFor(_sameIdentityMeter, "cratis.test.duration").ShouldContainOnly(0.4);
    [Fact] void should_distinguish_scoped_meters_with_the_same_name_and_version() => ValuesFor(_sameIdentityMeter, "cratis.test.scoped.duration").ShouldContainOnly(0.4);
    [Fact] void should_reuse_each_meters_unscoped_histogram() => _instruments.Count(_ => _.Name == "cratis.test.duration").ShouldEqual(3);
    [Fact] void should_reuse_each_meters_scoped_histogram() => _instruments.Count(_ => _.Name == "cratis.test.scoped.duration").ShouldEqual(3);

    IEnumerable<double> ValuesFor(Meter meter, string name) => _measurements.Where(_ => ReferenceEquals(_.Meter, meter) && _.Name == name).Select(_ => _.Value);
}
