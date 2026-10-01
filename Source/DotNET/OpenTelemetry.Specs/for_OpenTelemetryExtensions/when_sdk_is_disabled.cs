// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_sdk_is_disabled : given.a_clean_environment
{
    HostApplicationBuilder _builder;

    void Establish()
    {
        Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", "true");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317");
        _builder = Host.CreateApplicationBuilder();
    }

    void Because() => _builder.AddCratisOpenTelemetry();

    [Fact] void should_not_register_a_tracer_provider() => _builder.Services.Any(descriptor => descriptor.ServiceType == typeof(TracerProvider)).ShouldBeFalse();
}
