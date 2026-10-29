// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.Roslyn.Specs.for_ActivityScopeUsingAnalyzer;

public class when_assigned_to_a_field_without_using : Specification
{
    System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> _diagnostics;

    void Because() => _diagnostics = AnalyzerRunner.Run(
        @"
using Cratis.Traces;

namespace TestApp;

public class OrderService;

public static class OrderTraces
{
    public static IActivityScope<OrderService> ProcessOrder(IActivitySource<OrderService> source, string orderId) => throw null!;
}

public class Consumer(IActivitySource<OrderService> source)
{
    readonly IActivitySource<OrderService> _source = source;
    IActivityScope<OrderService>? _scope;

    public void Process()
    {
        _scope = OrderTraces.ProcessOrder(_source, ""42"");
    }
}
",
        new ActivityScopeUsingAnalyzer());

    [Fact] void should_report_crt0001() => _diagnostics.Any(_ => _.Id == ActivityScopeUsingAnalyzer.DiagnosticId).ShouldBeTrue();
}
