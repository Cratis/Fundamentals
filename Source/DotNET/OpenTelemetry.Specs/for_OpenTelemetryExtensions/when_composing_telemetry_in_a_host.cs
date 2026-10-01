// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_composing_telemetry_in_a_host : given.a_clean_environment
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    void should_preserve_configuration_and_honor_settings_registered_after_composition(bool web)
    {
        IHostApplicationBuilder builder = web ? WebApplication.CreateBuilder() : Host.CreateApplicationBuilder();
        builder.Services.AddOpenTelemetry().WithCratis();
        using var json = new MemoryStream(Encoding.UTF8.GetBytes("""{ "Application": { "FromAppSettings": "appsettings-value" }, "OTEL_SERVICE_NAME": "host-service", "OTEL_TRACES_SAMPLER": "always_off", "OTEL_METRIC_EXPORT_INTERVAL": "12345" }"""));
        builder.Configuration.AddJsonStream(json);
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Application:FromMemory"] = "in-memory-value"
        });
        using var host = web ? ((WebApplicationBuilder)builder).Build() : ((HostApplicationBuilder)builder).Build();
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        ReferenceEquals(configuration, builder.Configuration).ShouldBeTrue();
        configuration["Application:FromAppSettings"].ShouldEqual("appsettings-value");
        configuration["Application:FromMemory"].ShouldEqual("in-memory-value");
        var traces = host.Services.GetRequiredService<TracerProvider>();
        traces.GetResource().Attributes.Single(attribute => attribute.Key == "service.name").Value.ShouldEqual("host-service");
        using var source = new ActivitySource("Cratis.Test.HostComposition");
        using var activity = source.StartActivity("cratis.test.sample");
        activity!.IsAllDataRequested.ShouldBeFalse();
        activity.Recorded.ShouldBeFalse();
        host.Services.GetRequiredService<IOptions<PeriodicExportingMetricReaderOptions>>().Value.ExportIntervalMilliseconds.ShouldEqual(12345);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    void should_keep_application_configuration_with_separate_telemetry_configuration(bool web)
    {
        IHostApplicationBuilder builder = web ? WebApplication.CreateBuilder() : Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Application"] = "application-connection",
            ["OTEL_SERVICE_NAME"] = "application-service"
        });
        var telemetry = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_SERVICE_NAME"] = "telemetry-service"
        }).Build();
        builder.Services.AddOpenTelemetry().WithCratis(telemetry);
        using var host = web ? ((WebApplicationBuilder)builder).Build() : ((HostApplicationBuilder)builder).Build();
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        ReferenceEquals(configuration, builder.Configuration).ShouldBeTrue();
        configuration.GetConnectionString("Application").ShouldEqual("application-connection");
        configuration["OTEL_SERVICE_NAME"].ShouldEqual("application-service");
        host.Services.GetRequiredService<TracerProvider>().GetResource().Attributes.Single(attribute => attribute.Key == "service.name").Value.ShouldEqual("telemetry-service");
    }
}
