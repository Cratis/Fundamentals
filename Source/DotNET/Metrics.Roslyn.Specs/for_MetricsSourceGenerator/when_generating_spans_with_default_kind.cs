// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_generating_spans_with_default_kind : Specification
{
    string _generatedSource = string.Empty;

    void Establish()
    {
        var result = GeneratorRunner.Run(@"
using Cratis.Traces;

namespace TestApp;

public class OrderService;

static partial class OrderTraces
{
    [Span(""order.validate"")]
    internal static partial IActivityScope<OrderService> ValidateOrder(
        IActivitySource<OrderService> source,
        string orderId);
}
");

        _generatedSource = result.GeneratedTrees.Single().GetText().ToString();
    }

    [Fact] void should_use_internal_activity_kind() => _generatedSource.ShouldContain("StartActivity(\"order.validate\", ActivityKind.Internal)");
}
