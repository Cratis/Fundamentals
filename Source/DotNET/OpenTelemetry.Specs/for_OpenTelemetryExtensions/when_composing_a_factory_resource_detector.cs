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

public class when_composing_a_factory_resource_detector : given.a_clean_environment
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    void should_resolve_the_detector_once_per_provider_and_preserve_its_identity(bool overrideIdentity)
    {
        var builder = Host.CreateApplicationBuilder();
        if (overrideIdentity)
        {
            builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"] = "service.version=3.0";
            builder.Configuration["OTEL_SERVICE_NAME"] = "configured";
        }
        var detector = new service_detector();
        builder.Services.AddSingleton(detector);
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddDetector(provider => provider.GetRequiredService<service_detector>()))
            .WithCratis(options =>
            {
                options.ServiceName = "fallback";
                options.ServiceVersion = "1.0";
            });
        using var host = builder.Build();
        Resource[] resources =
        [
            host.Services.GetRequiredService<TracerProvider>().GetResource(),
            host.Services.GetRequiredService<MeterProvider>().GetResource(),
            host.Services.GetRequiredService<LoggerProvider>().GetResource()
        ];
        foreach (var resource in resources)
        {
            resource.Attributes.Single(attribute => attribute.Key == "service.name").Value.ShouldEqual(overrideIdentity ? "configured" : "detected");
            resource.Attributes.Single(attribute => attribute.Key == "service.version").Value.ShouldEqual(overrideIdentity ? "3.0" : "2.0");
        }
        detector.Calls.ShouldEqual(3);
    }

    sealed class service_detector : IResourceDetector
    {
        public int Calls { get; private set; }

        public Resource Detect()
        {
            Calls++;
            return new Resource([new("service.name", "detected"), new("service.version", "2.0")]);
        }
    }
}
