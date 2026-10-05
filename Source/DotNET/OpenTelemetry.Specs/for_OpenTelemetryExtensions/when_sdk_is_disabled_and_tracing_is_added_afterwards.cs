// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_sdk_is_disabled_and_tracing_is_added_afterwards : given.a_clean_environment
{
    HostApplicationBuilder _builder;
    IConfiguration _configuration;
    IHost _host;
    ActivitySource _source;
    Meter _meter;
    Activity? _activity;
    Counter<long> _counter;
    recording_exporter<LogRecord> _logs;

    void Establish()
    {
        _builder = Host.CreateApplicationBuilder();
        _builder.Configuration["OTEL_SDK_DISABLED"] = "false";
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_SDK_DISABLED"] = "true"
        }).Build();
        _source = new ActivitySource("Application.Test.Disabled");
        _meter = new Meter("Application.Test.Disabled");
        _counter = _meter.CreateCounter<long>("application.test.count");
        _logs = new recording_exporter<LogRecord>();
    }

    void Because()
    {
        _builder.Services.AddOpenTelemetry().WithCratis(_configuration)
            .WithTracing(tracing => tracing.AddSource("Application.Test.Disabled").SetSampler(new AlwaysOnSampler()))
            .WithMetrics(metrics => metrics.AddMeter("Application.Test.Disabled").AddReader(new PeriodicExportingMetricReader(new recording_exporter<Metric>())))
            .WithLogging(logging => logging.AddProcessor(new SimpleLogRecordExportProcessor(_logs)));
        _host = _builder.Build();
        _host.Services.GetRequiredService<TracerProvider>();
        _host.Services.GetRequiredService<MeterProvider>();
        var logging = _host.Services.GetRequiredService<LoggerProvider>();
        _activity = _source.StartActivity("application.test");
        var logger = _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Application.Test.Disabled");
        LoggerMessage.Define(LogLevel.Information, new EventId(1), "Application log")(logger, null);
        logging.ForceFlush(5_000);
    }

    void Destroy()
    {
        _activity?.Dispose();
        _source.Dispose();
        _meter.Dispose();
        _host.Dispose();
    }

    [Fact] void should_not_record_an_activity() => _activity.ShouldBeNull();
    [Fact] void should_not_enable_the_added_meter() => _counter.Enabled.ShouldBeFalse();
    [Fact] void should_not_export_the_added_logs() => _logs.Count.ShouldEqual(0);
    [Fact] void should_not_change_the_host_configuration() => _host.Services.GetRequiredService<IConfiguration>()["OTEL_SDK_DISABLED"].ShouldEqual("false");

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
