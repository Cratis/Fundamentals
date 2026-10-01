// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_resolving_keyed_services_in_provider_callbacks : given.a_clean_environment
{
    [Fact]
    void should_preserve_keyed_service_resolution()
    {
        var builder = Host.CreateApplicationBuilder();
        var expected = new object();
        object? resolved = null;
        builder.Services.AddKeyedSingleton("application", expected);
        builder.Services.ConfigureOpenTelemetryTracerProvider((provider, _) => resolved = provider.GetRequiredKeyedService<object>("application"));
        builder.Services.AddOpenTelemetry().WithCratis();
        using var host = builder.Build();
        _ = host.Services.GetRequiredService<TracerProvider>();
        ReferenceEquals(resolved, expected).ShouldBeTrue();
    }
}
