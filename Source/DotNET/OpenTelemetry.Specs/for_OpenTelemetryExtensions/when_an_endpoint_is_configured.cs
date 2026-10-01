// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_an_endpoint_is_configured : given.a_clean_environment
{
    HostApplicationBuilder _builder;

    void Establish()
    {
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://collector:4317");
        _builder = Host.CreateApplicationBuilder();
    }

    void Because() => _builder.AddCratisOpenTelemetry();

    [Fact] void should_register_the_otlp_exporter() => _builder.Services.Any(descriptor => descriptor.ServiceType == typeof(IOptionsFactory<OtlpExporterOptions>)).ShouldBeTrue();
}
