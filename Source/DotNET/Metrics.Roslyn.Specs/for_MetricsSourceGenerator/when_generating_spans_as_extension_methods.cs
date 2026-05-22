// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_generating_spans_as_extension_methods : Specification
{
    Microsoft.CodeAnalysis.GeneratorDriverRunResult _result;
    string _generatedSource = string.Empty;

    void Because()
    {
        _result = GeneratorRunner.Run(@"
using Cratis.Traces;
using System.Diagnostics;

namespace TestApp;

public class OrderService;

static partial class OrderTraces
{
    [Span(""order.process"", ActivityKind.Server)]
    internal static partial IActivityScope<OrderService> ProcessOrder(
        this IActivitySource<OrderService> source,
        string orderId);
}
");

        _generatedSource = _result.GeneratedTrees.Single().GetText().ToString();
    }

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_preserve_the_extension_method_signature() => _generatedSource.ShouldContain("this IActivitySource<OrderService> source");
    [Fact] void should_start_the_activity_from_the_extension_target() => _generatedSource.ShouldContain("var activity = source.ActualSource.StartActivity(\"order.process\", ActivityKind.Server);");
}
