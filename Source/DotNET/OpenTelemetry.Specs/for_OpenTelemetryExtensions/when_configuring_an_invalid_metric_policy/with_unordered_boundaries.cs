// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_configuring_an_invalid_metric_policy;

public class with_unordered_boundaries : given.a_clean_environment
{
    readonly List<Exception> _errors = [];

    void Because()
    {
        foreach (var boundaries in new double[][] { [1, 0.5], [1, 1], [double.NaN], [double.PositiveInfinity], [double.NegativeInfinity] })
        {
            _errors.Add(Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(options => options.MetricPolicies["spec.invalid"] = new() { Boundaries = boundaries })));
            _errors.Add(Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(options => options.SecondsHistogramBoundaries = boundaries)));
        }
    }

    [Fact] void should_fail_with_invalid_metric_policy() => _errors.TrueForAll(error => error.GetType() == typeof(InvalidMetricPolicy)).ShouldBeTrue();
    [Fact] void should_name_the_setting() => _errors.TrueForAll(error => error.Message.Contains("Boundaries", StringComparison.Ordinal)).ShouldBeTrue();
}
