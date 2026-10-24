// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_configuring_propagation : given.a_configured_environment
{
    readonly TextMapPropagator _previous = Propagators.DefaultTextMapPropagator;

    [Fact] void should_include_trace_context_by_default() => Fields(null).ShouldContain("traceparent");
    [Fact] void should_include_baggage_by_default() => Fields(null).ShouldContain("baggage");
    [Fact] void should_allow_disabling_propagation() => Fields("none").ShouldBeEmpty();
    [Fact] void should_allow_baggage_without_trace_context() => Fields("baggage").ShouldNotContain("traceparent");
    [Fact] void should_allow_trace_context_without_baggage() => Fields("tracecontext").ShouldNotContain("baggage");
    [Fact]
    void should_prefer_the_environment_over_configuration()
    {
        Environment.SetEnvironmentVariable("OTEL_PROPAGATORS", "none");
        Fields("tracecontext,baggage").ShouldBeEmpty();
    }

    string[] Fields(string? propagators)
    {
        // Start from a different global setting so assertions prove shared setup changed it.
        Sdk.SetDefaultTextMapPropagator(new BaggagePropagator());
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["OTEL_PROPAGATORS"] = propagators }).Build();
            var builder = CreateBuilder(configuration);
            builder.AddCratisOpenTelemetry();
            using var services = builder.Services.BuildServiceProvider();
            using var provider = services.GetRequiredService<TracerProvider>();
            return [.. Propagators.DefaultTextMapPropagator.Fields];
        }
        finally
        {
            Sdk.SetDefaultTextMapPropagator(_previous);
        }
    }
}
