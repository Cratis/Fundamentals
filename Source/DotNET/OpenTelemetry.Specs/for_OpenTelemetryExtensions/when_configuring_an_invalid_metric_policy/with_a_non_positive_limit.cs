// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_configuring_an_invalid_metric_policy;

public class with_a_non_positive_limit : given.a_clean_environment
{
    readonly List<Exception> _errors = [];

    void Because()
    {
        foreach (var limit in new[] { 0, -1 })
        {
            _errors.Add(Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(options => options.MetricCardinalityLimit = limit)));
            _errors.Add(Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(options => options.MetricPolicies["spec.invalid"] = new() { CardinalityLimit = limit })));
        }
    }

    [Fact] void should_fail_with_invalid_metric_policy() => _errors.TrueForAll(error => error.GetType() == typeof(InvalidMetricPolicy)).ShouldBeTrue();
    [Fact] void should_name_the_setting() => _errors.TrueForAll(error => error.Message.Contains("CardinalityLimit", StringComparison.Ordinal)).ShouldBeTrue();
}
