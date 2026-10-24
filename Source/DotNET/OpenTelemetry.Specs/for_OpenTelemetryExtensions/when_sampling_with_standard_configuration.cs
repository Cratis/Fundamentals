// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_sampling_with_standard_configuration : given.a_configured_environment
{
    [Fact] void should_reject_a_missing_ratio() => AssertInvalidRatio(null);
    [Fact] void should_reject_a_non_numeric_ratio() => AssertInvalidRatio("invalid");
    [Fact] void should_reject_a_negative_ratio() => AssertInvalidRatio("-0.1");
    [Fact] void should_reject_a_ratio_above_one() => AssertInvalidRatio("1.1");
    [Fact] void should_reject_a_non_finite_ratio() => AssertInvalidRatio("NaN");
    [Fact] void should_reject_an_infinite_ratio() => AssertInvalidRatio("Infinity");
    [Fact] void should_honor_always_off() => IsRecorded("always_off", null).ShouldBeFalse();
    [Fact] void should_honor_always_on() => IsRecorded("always_on", null).ShouldBeTrue();
    [Fact] void should_honor_a_zero_ratio() => IsRecorded("traceidratio", "0").ShouldBeFalse();
    [Fact] void should_honor_a_full_ratio() => IsRecorded("traceidratio", "1").ShouldBeTrue();
    [Fact] void should_keep_parent_sampling_with_parentbased_always_off() => IsRecorded("parentbased_always_off", null, true).ShouldBeTrue();
    [Fact] void should_honor_a_full_parentbased_ratio() => IsRecorded("parentbased_traceidratio", "1").ShouldBeTrue();
    [Fact] void should_honor_a_zero_parentbased_ratio() => IsRecorded("parentbased_traceidratio", "0").ShouldBeFalse();
    [Fact] void should_keep_a_sampled_parent_with_a_zero_ratio() => IsRecorded("parentbased_traceidratio", "0", sampledParent: true).ShouldBeTrue();
    [Fact]
    void should_prefer_the_environment_ratio_over_configuration()
    {
        var previous = Environment.GetEnvironmentVariable("OTEL_TRACES_SAMPLER_ARG");
        try
        {
            Environment.SetEnvironmentVariable("OTEL_TRACES_SAMPLER_ARG", "0");
            IsRecorded("traceidratio", "1").ShouldBeFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("OTEL_TRACES_SAMPLER_ARG", previous);
        }
    }
    [Fact] void should_not_override_environment_sampling_in_an_application_callback() => IsRecorded("always_off", null, configureApplicationSampler: true).ShouldBeFalse();

    static void AssertInvalidRatio(string? ratio)
    {
        foreach (var sampler in new[] { "traceidratio", "parentbased_traceidratio" })
        {
            var exception = Catch.Exception(() => IsRecorded(sampler, ratio));
            exception.ShouldNotBeNull();
            exception.GetType().ShouldEqual(typeof(InvalidOpenTelemetrySamplerArgument));
            exception.Message.ShouldContain("OTEL_TRACES_SAMPLER_ARG");
            exception.Message.ShouldContain(ratio ?? "<missing>");
        }
    }

    static bool IsRecorded(string sampler, string? ratio, bool sampledParent = false, bool configureApplicationSampler = false)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_TRACES_SAMPLER"] = sampler,
            ["OTEL_TRACES_SAMPLER_ARG"] = ratio
        }).Build();
        var builder = CreateBuilder(configuration);
        builder.AddCratisOpenTelemetry(options =>
        {
            if (configureApplicationSampler) options.ConfigureTracing = tracing => tracing.SetSampler(new AlwaysOnSampler());
        });
        using var services = builder.Services.BuildServiceProvider();
        using var provider = services.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource("Cratis.Sampling.Specs");
        var parent = sampledParent ? new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded, isRemote: true) : default;
        using var activity = source.StartActivity("sample", ActivityKind.Internal, parent);
        return activity?.Recorded ?? false;
    }
}
