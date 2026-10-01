// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_sampling_from_application_configuration : given.a_clean_environment
{
    [Theory]
    [InlineData("always_on", "0", true)]
    [InlineData("always_off", "1", false)]
    [InlineData("traceidratio", "0", false)]
    [InlineData("traceidratio", "1", true)]
    [InlineData("parentbased_always_on", "0", true)]
    [InlineData("parentbased_always_off", "1", false)]
    [InlineData("parentbased_traceidratio", "0", false)]
    [InlineData("parentbased_traceidratio", "1", true)]
    void should_honor_standard_root_sampling_choices(string sampler, string ratio, bool sampled)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddOpenTelemetry().WithCratis();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_TRACES_SAMPLER"] = sampler,
            ["OTEL_TRACES_SAMPLER_ARG"] = ratio
        });
        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource("Cratis.Test.HostSampler");
        using var activity = source.StartActivity("sample", ActivityKind.Internal, default(ActivityContext));
        activity!.Recorded.ShouldEqual(sampled);
    }

    [Theory]
    [InlineData("ALWAYS_OFF", false)]
    [InlineData("PaReNtBaSeD_AlWaYs_OfF", false)]
    [InlineData("ALWAYS_OFF", true)]
    [InlineData("PaReNtBaSeD_AlWaYs_OfF", true)]
    void should_honor_case_insensitive_environment_sampling_over_configuration(string sampler, bool separate)
    {
        Environment.SetEnvironmentVariable("OTEL_TRACES_SAMPLER", sampler);
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["OTEL_TRACES_SAMPLER"] = "always_on";
        if (separate)
        {
            var telemetry = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OTEL_TRACES_SAMPLER"] = "always_on"
            }).Build();
            builder.Services.AddOpenTelemetry().WithCratis(telemetry);
        }
        else
        {
            builder.Services.AddOpenTelemetry().WithCratis();
        }
        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource("Cratis.Test.HostSampler");
        using var activity = source.StartActivity("sample", ActivityKind.Internal, default(ActivityContext));
        activity!.Recorded.ShouldBeFalse();
    }

    [Fact]
    void should_allow_application_callbacks_to_override_the_shared_sampler()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["OTEL_TRACES_SAMPLER"] = "always_off";
        builder.Services.AddOpenTelemetry().WithCratis(options => options.ConfigureTracing = tracing => tracing.SetSampler(new AlwaysOnSampler()));
        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource("Cratis.Test.HostSampler");
        using var activity = source.StartActivity("sample", ActivityKind.Internal, default(ActivityContext));
        activity!.Recorded.ShouldBeTrue();
    }
}
