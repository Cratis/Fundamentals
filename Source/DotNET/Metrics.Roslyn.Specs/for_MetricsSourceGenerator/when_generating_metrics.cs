// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_generating_metrics : Specification
{
    Microsoft.CodeAnalysis.GeneratorDriverRunResult _result;
    string _generatedSource = string.Empty;

    void Because()
    {
        _result = GeneratorRunner.Run(@"
using Cratis.Metrics;

namespace TestApp;

public static partial class Metrics
{
    [Counter<int>(""requests"", ""Number of requests"")]
    public static partial void Count(IMeter<object> meter);

    [Counter<long>(""scoped_requests"", ""Number of scoped requests"")]
    public static partial void CountScoped(IMeterScope<object> scope, long increment);

    [Gauge<double>(""temperature"", ""Current temperature"")]
    public static partial void Record(IMeter<object> meter, double measurement);

    [Gauge<int>(""scoped_temperature"", ""Current scoped temperature"")]
    public static partial void RecordScoped(IMeterScope<object> scope, int measurement);
}
");
        _generatedSource = _result.GeneratedTrees.Single().GetText().ToString();
    }

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_create_a_counter_with_description_and_no_unit() => _generatedSource.ShouldContain("histogramMeter.CreateCounter<int>(name: \"requests\", unit: null, description: \"Number of requests\")");
    [Fact] void should_create_a_scoped_counter_with_description_and_no_unit() => _generatedSource.ShouldContain("histogramMeter.CreateCounter<long>(name: \"scoped_requests\", unit: null, description: \"Number of scoped requests\")");
    [Fact] void should_create_a_gauge_with_description_and_no_unit() => _generatedSource.ShouldContain("histogramMeter.CreateGauge<double>(name: \"temperature\", unit: null, description: \"Current temperature\")");
    [Fact] void should_create_a_scoped_gauge_with_description_and_no_unit() => _generatedSource.ShouldContain("histogramMeter.CreateGauge<int>(name: \"scoped_temperature\", unit: null, description: \"Current scoped temperature\")");
}
