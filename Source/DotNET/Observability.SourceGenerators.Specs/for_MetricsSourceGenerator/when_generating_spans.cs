// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_generating_spans : Specification
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
        IActivitySource<OrderService> source,
    string orderID,
        string customerId);
}
");
        _generatedSource = _result.GeneratedTrees.Single().GetText().ToString();
    }

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_start_activity_with_declared_name_and_kind() => _generatedSource.ShouldContain("StartActivity(\"order.process\", ActivityKind.Server)");
    [Fact] void should_add_tags_from_additional_parameters() => _generatedSource.ShouldContain("SetTag(\"order_id\", orderID)");
    [Fact] void should_return_an_activity_scope() => _generatedSource.ShouldContain("return new global::Cratis.Traces.ActivityScope<OrderService>(activity);");
}
