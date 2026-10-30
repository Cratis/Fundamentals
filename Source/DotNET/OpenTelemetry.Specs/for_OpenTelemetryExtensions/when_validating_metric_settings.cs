// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_validating_metric_settings : given.a_clean_environment
{
    readonly List<Exception> _errors = [];

    void Because()
    {
        Action<CratisOpenTelemetryOptions>[] invalidSettings =
        [
            options => options.MetricCardinalityLimit = 0,
            options => options.MetricPolicies["spec.invalid"] = new() { CardinalityLimit = -1 },
            options => options.SecondsHistogramBoundaries = [double.NaN],
            options => options.MetricPolicies["spec.invalid"] = new() { Boundaries = [1, 0] },
            options => options.MetricPolicies["spec.invalid"] = new() { TagKeys = [" "] }
        ];
        foreach (var enabled in new[] { false, true })
        {
            foreach (var configure in invalidSettings)
            {
                _errors.Add(Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(options =>
                {
                    options.EnableMetricViews = enabled;
                    configure(options);
                })));
            }
        }
    }

    [Fact] void should_reject_each_invalid_setting_in_both_modes() => _errors.Count.ShouldEqual(10);
    [Fact] void should_validate_before_registration_regardless_of_opt_in() => _errors.ShouldEachConformTo(error => error is InvalidMetricPolicy);
}
