// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_configuring_an_invalid_metric_policy;

public class with_blank_tag_keys : given.a_clean_environment
{
    readonly List<Exception> _errors = [];

    void Because()
    {
        foreach (var tagKey in new[] { null, "", " " })
        {
            _errors.Add(Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(options =>
            {
                options.EnableMetricViews = true;
                options.MetricPolicies["spec.invalid"] = new() { TagKeys = [tagKey!] };
            })));
        }
    }

    [Fact] void should_fail_with_invalid_metric_policy() => _errors.TrueForAll(error => error.GetType() == typeof(InvalidMetricPolicy)).ShouldBeTrue();
    [Fact] void should_name_the_setting() => _errors.TrueForAll(error => error.Message.Contains("TagKeys", StringComparison.Ordinal)).ShouldBeTrue();
}
