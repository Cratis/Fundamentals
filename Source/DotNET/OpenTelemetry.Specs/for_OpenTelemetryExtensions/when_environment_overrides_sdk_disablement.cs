// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_environment_overrides_sdk_disablement : given.a_recording_exporter
{
    [Theory]
    [InlineData(false, "host", false)]
    [InlineData(true, "host", false)]
    [InlineData(false, "composition", false)]
    [InlineData(true, "composition", false)]
    [InlineData(false, "separate", false)]
    [InlineData(true, "separate", false)]
    [InlineData(false, "host", true)]
    [InlineData(true, "host", true)]
    [InlineData(false, "composition", true)]
    [InlineData(true, "composition", true)]
    [InlineData(false, "separate", true)]
    [InlineData(true, "separate", true)]
    void should_apply_the_environment_decision_to_all_providers_without_changing_application_configuration(bool web, string setup, bool disabled)
    {
        Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", disabled ? "true" : "false");
        IHostApplicationBuilder builder = web ? WebApplication.CreateBuilder() : Host.CreateApplicationBuilder();
        foreach (var descriptor in _services)
        {
            builder.Services.Add(descriptor);
        }
        var traces = new recording_exporter<Activity>();
        var metrics = new recording_exporter<Metric>();
        var logs = new recording_exporter<LogRecord>();
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource("Application.Test").AddProcessor(new SimpleActivityExportProcessor(traces)))
            .WithMetrics(meter => meter.AddMeter("Application.Test").AddReader(new BaseExportingMetricReader(metrics)))
            .WithLogging(logging => logging.AddProcessor(new SimpleLogRecordExportProcessor(logs)));
        builder.Configuration.AddConfiguration(_configuration);
        builder.Configuration["OTEL_SDK_DISABLED"] = disabled ? "false" : "true";
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
            _configuration["OTEL_SDK_DISABLED"] = disabled ? "false" : "true";
            builder.Services.AddOpenTelemetry().WithCratis(_configuration);
        }
        using var host = web ? ((WebApplicationBuilder)builder).Build() : ((HostApplicationBuilder)builder).Build();
        ExportSignals(host.Services);
        using var source = new ActivitySource("Application.Test");
        using (var activity = source.StartActivity("application.test"))
        {
            if (disabled)
            {
                activity.ShouldBeNull();
            }
            else
            {
                activity.ShouldNotBeNull();
            }
        }
        using var meter = new Meter("Application.Test");
        meter.CreateCounter<long>("application.test.count").Add(1);
        host.Services.GetRequiredService<MeterProvider>().ForceFlush(5_000);
        (traces.Count > 0).ShouldEqual(!disabled);
        (metrics.Count > 0).ShouldEqual(!disabled);
        (logs.Count > 0).ShouldEqual(!disabled);
        ReferenceEquals(host.Services.GetRequiredService<IConfiguration>(), builder.Configuration).ShouldBeTrue();
        builder.Configuration["OTEL_SDK_DISABLED"].ShouldEqual(disabled ? "false" : "true");
        _clientsCreated.ShouldEqual(disabled ? 0 : 3);
        if (disabled)
        {
            _requests.ShouldBeEmpty();
        }
        else
        {
            _requests.Select(request => request.Endpoint).Distinct().Count().ShouldEqual(3);
        }
    }

    sealed class recording_exporter<T> : BaseExporter<T>
        where T : class
    {
        public int Count { get; private set; }

        public override ExportResult Export(in Batch<T> batch)
        {
            Count += (int)batch.Count;
            return ExportResult.Success;
        }
    }
}
