// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.Roslyn.Specs.for_MetricsSourceGenerator;

public class when_the_return_type_is_invalid_for_a_span : Specification
{
    Microsoft.CodeAnalysis.GeneratorDriverRunResult _result;

    void Because() => _result = GeneratorRunner.Run(@"
using Cratis.Traces;

namespace TestApp;

public class OrderService;

static partial class OrderTraces
{
    [Span(""order.validate"")]
    internal static partial void ValidateOrder(IActivitySource<OrderService> source, string orderId);
}
");

    [Fact] void should_report_traces003() => _result.Diagnostics.Any(_ => _.Id == "TRACES003").ShouldBeTrue();
}
