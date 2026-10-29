// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_filtering_health_requests : given.a_configured_environment
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/healthz")]
    [InlineData("/ready")]
    [InlineData("/readyz")]
    [InlineData("/readiness")]
    void should_skip_probe_requests(string path) => IsIncluded(path).ShouldBeFalse();

    [Fact] void should_trace_product_requests() => IsIncluded("/orders").ShouldBeTrue();
    [Fact] void should_not_filter_similar_product_paths() => IsIncluded("/healthcare").ShouldBeTrue();
    [Fact] void should_allow_a_product_override() => IsIncluded("/health", true).ShouldBeTrue();

    static bool IsIncluded(string path, bool overrideFilter = false)
    {
        var builder = CreateBuilder();
        builder.AddCratisOpenTelemetry(options =>
        {
            if (overrideFilter) options.ConfigureTracing = tracing => tracing.AddAspNetCoreInstrumentation(instrumentation => instrumentation.Filter = _ => true);
        });
        using var services = builder.Services.BuildServiceProvider();
        services.GetRequiredService<TracerProvider>();
        var instrumentation = services.GetRequiredService<IOptions<AspNetCoreTraceInstrumentationOptions>>().Value;
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return instrumentation.Filter?.Invoke(context) ?? true;
    }
}
