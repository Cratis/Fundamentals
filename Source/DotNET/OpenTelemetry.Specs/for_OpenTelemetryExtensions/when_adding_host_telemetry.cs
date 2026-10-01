// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_adding_host_telemetry : given.a_clean_environment
{
    IHost _host;
    HostApplicationBuilder _builder;
    TracerProvider _traces;
    MeterProvider _metrics;
    OpenTelemetryLoggerOptions _logs;

    void Establish() => _builder = Host.CreateApplicationBuilder();

    void Because()
    {
        _builder.AddCratisOpenTelemetry(options =>
        {
            options.ServiceName = "my-app";
            options.ServiceVersion = "1.2.3";
        });
        _host = _builder.Build();
        _traces = _host.Services.GetRequiredService<TracerProvider>();
        _metrics = _host.Services.GetRequiredService<MeterProvider>();
        _logs = _host.Services.GetRequiredService<IOptions<OpenTelemetryLoggerOptions>>().Value;
    }

    void Destroy() => _host.Dispose();

    [Fact] void should_set_the_default_service_name() => _traces.GetResource().Attributes.Single(attribute => attribute.Key == "service.name").Value.ShouldEqual("my-app");
    [Fact] void should_set_the_service_version() => _metrics.GetResource().Attributes.Single(attribute => attribute.Key == "service.version").Value.ShouldEqual("1.2.3");
    [Fact] void should_include_log_scopes() => _logs.IncludeScopes.ShouldBeTrue();
    [Fact] void should_include_formatted_log_messages() => _logs.IncludeFormattedMessage.ShouldBeTrue();
    [Fact] void should_not_register_an_otlp_exporter_without_an_endpoint() => _builder.Services.Any(descriptor => descriptor.ServiceType == typeof(IOptionsFactory<OtlpExporterOptions>)).ShouldBeFalse();
    [Fact] void should_subscribe_to_future_cratis_sources()
    {
        using var source = new ActivitySource("Cratis.Future.Product");
        using var activity = source.StartActivity("cratis.future.operation");
        activity.ShouldNotBeNull();
    }
    [Fact] void should_not_subscribe_to_unkeyed_application_sources()
    {
        using var source = new ActivitySource("MyApplication.RequestHandler");
        using var activity = source.StartActivity("request");
        activity.ShouldBeNull();
    }
}
