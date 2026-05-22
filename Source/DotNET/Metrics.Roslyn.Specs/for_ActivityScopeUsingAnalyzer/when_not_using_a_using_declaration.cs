// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Metrics.Roslyn.Specs.for_ActivityScopeUsingAnalyzer;

public class when_not_using_a_using_declaration : Specification
{
    ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> _diagnostics;

    void Establish()
    {
        var compilation = CompilationFactory.CreateCompilation(@"
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

    public void Process()
    {
        var span = OrderTraces.ProcessOrder(_source, ""42"");
    }
}
");

        var analyzer = new ActivityScopeUsingAnalyzer();
        _diagnostics = compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
    }

    [Fact] void should_report_crt0001() => _diagnostics.Any(_ => _.Id == ActivityScopeUsingAnalyzer.DiagnosticId).ShouldBeTrue();
}
