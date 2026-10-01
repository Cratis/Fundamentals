// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_recording_counters_and_gauges_with_multiple_meters : Specification
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
    [Counter<double>(""cratis.test.count"", ""Count"")]
    public static partial void Count(IMeter<object> meter, double value, string actualMeter, string histogramMeter);

    [Counter<double>(""cratis.test.scoped.count"", ""Scoped count"")]
    public static partial void ScopedCount(IMeterScope<object> scope, double value, string actualMeter, string histogramMeter);

    [Gauge<double>(""cratis.test.gauge"", ""Gauge"")]
    public static partial void Gauge(IMeter<object> meter, double value, string actualMeter, string histogramMeter);

    [Gauge<double>(""cratis.test.scoped.gauge"", ""Scoped gauge"")]
    public static partial void ScopedGauge(IMeterScope<object> scope, double value, string actualMeter, string histogramMeter);
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

        Record(firstMeter, 1.0);
        Record(secondMeter, 2.0);
        firstMeter.Dispose();
        Record(secondMeter, 3.0);
        Record(sameIdentityMeter, 4.0);

        void Record(Meter meter, double value)
        {
            var typedMeter = new Meter<object>(meter);
            using var scope = new MeterScope<object>(typedMeter, new Dictionary<string, object> { ["cratis.test.scope"] = "scoped" });
            telemetry.GetMethod("Count")!.Invoke(null, [typedMeter, value, "actual", "factory"]);
            telemetry.GetMethod("ScopedCount")!.Invoke(null, [scope, value, "actual", "factory"]);
            telemetry.GetMethod("Gauge")!.Invoke(null, [typedMeter, value, "actual", "factory"]);
            telemetry.GetMethod("ScopedGauge")!.Invoke(null, [scope, value, "actual", "factory"]);
        }
    }

    [Fact] void should_record_unscoped_counts_through_the_first_meter() => ValuesFor(_firstMeter, "cratis.test.count").ShouldContainOnly(1.0);
    [Fact] void should_record_scoped_counts_through_the_first_meter() => ValuesFor(_firstMeter, "cratis.test.scoped.count").ShouldContainOnly(1.0);
    [Fact] void should_record_unscoped_gauges_through_the_first_meter() => ValuesFor(_firstMeter, "cratis.test.gauge").ShouldContainOnly(1.0);
    [Fact] void should_record_scoped_gauges_through_the_first_meter() => ValuesFor(_firstMeter, "cratis.test.scoped.gauge").ShouldContainOnly(1.0);
    [Fact] void should_record_unscoped_counts_through_the_second_meter_after_disposing_the_first() => ValuesFor(_secondMeter, "cratis.test.count").ShouldContainOnly(2.0, 3.0);
    [Fact] void should_record_scoped_counts_through_the_second_meter_after_disposing_the_first() => ValuesFor(_secondMeter, "cratis.test.scoped.count").ShouldContainOnly(2.0, 3.0);
    [Fact] void should_record_unscoped_gauges_through_the_second_meter_after_disposing_the_first() => ValuesFor(_secondMeter, "cratis.test.gauge").ShouldContainOnly(2.0, 3.0);
    [Fact] void should_record_scoped_gauges_through_the_second_meter_after_disposing_the_first() => ValuesFor(_secondMeter, "cratis.test.scoped.gauge").ShouldContainOnly(2.0, 3.0);
    [Fact] void should_distinguish_unscoped_counters_with_the_same_meter_name_and_version() => ValuesFor(_sameIdentityMeter, "cratis.test.count").ShouldContainOnly(4.0);
    [Fact] void should_distinguish_scoped_counters_with_the_same_meter_name_and_version() => ValuesFor(_sameIdentityMeter, "cratis.test.scoped.count").ShouldContainOnly(4.0);
    [Fact] void should_distinguish_unscoped_gauges_with_the_same_meter_name_and_version() => ValuesFor(_sameIdentityMeter, "cratis.test.gauge").ShouldContainOnly(4.0);
    [Fact] void should_distinguish_scoped_gauges_with_the_same_meter_name_and_version() => ValuesFor(_sameIdentityMeter, "cratis.test.scoped.gauge").ShouldContainOnly(4.0);
    [Fact] void should_reuse_each_meters_unscoped_counter() => _instruments.Count(_ => _.Name == "cratis.test.count").ShouldEqual(3);
    [Fact] void should_reuse_each_meters_scoped_counter() => _instruments.Count(_ => _.Name == "cratis.test.scoped.count").ShouldEqual(3);
    [Fact] void should_reuse_each_meters_unscoped_gauge() => _instruments.Count(_ => _.Name == "cratis.test.gauge").ShouldEqual(3);
    [Fact] void should_reuse_each_meters_scoped_gauge() => _instruments.Count(_ => _.Name == "cratis.test.scoped.gauge").ShouldEqual(3);

    IEnumerable<double> ValuesFor(Meter meter, string name) => _measurements.Where(_ => ReferenceEquals(_.Meter, meter) && _.Name == name).Select(_ => _.Value);
}
