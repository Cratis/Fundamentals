// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_composing_an_existing_resource : given.a_clean_environment
{
    [Fact]
    void should_preserve_existing_service_identity_for_all_signals()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("billing", serviceVersion: "2.0"))
            .WithCratis();
        using var host = builder.Build();
        Resource[] resources =
        [
            host.Services.GetRequiredService<TracerProvider>().GetResource(),
            host.Services.GetRequiredService<MeterProvider>().GetResource(),
            host.Services.GetRequiredService<LoggerProvider>().GetResource()
        ];
        foreach (var resource in resources)
        {
            resource.Attributes.Single(attribute => attribute.Key == "service.name").Value.ShouldEqual("billing");
            resource.Attributes.Single(attribute => attribute.Key == "service.version").Value.ShouldEqual("2.0");
        }
    }
}
