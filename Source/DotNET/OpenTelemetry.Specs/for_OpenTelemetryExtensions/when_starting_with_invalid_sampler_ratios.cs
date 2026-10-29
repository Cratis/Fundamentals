// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_starting_with_invalid_sampler_ratios : given.a_configured_environment
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("-0.1")]
    [InlineData("1.1")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    async Task should_fail_host_startup_with_the_specific_exception(string? ratio)
    {
        foreach (var sampler in new[] { "traceidratio", "parentbased_traceidratio" })
        {
            foreach (var setup in new[] { "host", "composition", "separate" })
            {
                var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["OTEL_TRACES_SAMPLER"] = sampler,
                    ["OTEL_TRACES_SAMPLER_ARG"] = ratio
                }).Build();
                var builder = CreateBuilder(configuration);
                if (setup == "host")
                {
                    builder.AddCratisOpenTelemetry();
                }
                else if (setup == "composition")
                {
                    builder.Services.AddOpenTelemetry().WithCratis();
                }
                else
                {
                    builder.Services.AddOpenTelemetry().WithCratis(configuration);
                }
                var exception = await Catch.Exception(async () =>
                {
                    using var host = builder.Build();
                    await host.StartAsync();
                });
                exception.ShouldBeOfExactType<InvalidOpenTelemetrySamplerArgument>();
            }
        }
    }
}
