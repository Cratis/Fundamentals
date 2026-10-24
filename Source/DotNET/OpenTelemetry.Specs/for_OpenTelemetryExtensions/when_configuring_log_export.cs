// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_configuring_log_export : given.a_configured_environment
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    void should_honor_batch_configuration_and_environment_precedence(bool environment)
    {
        var settings = new Dictionary<string, string?>
        {
            ["OTEL_BLRP_MAX_QUEUE_SIZE"] = "256",
            ["OTEL_BLRP_MAX_EXPORT_BATCH_SIZE"] = "64",
            ["OTEL_BLRP_SCHEDULE_DELAY"] = "250",
            ["OTEL_BLRP_EXPORT_TIMEOUT"] = "500"
        };
        if (environment)
        {
            foreach (var (key, value) in settings) Environment.SetEnvironmentVariable(key, (int.Parse(value!) * 2).ToString());
        }
        var builder = CreateBuilder(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        builder.AddCratisOpenTelemetry();
        using var services = builder.Services.BuildServiceProvider();
        var batch = services.GetRequiredService<IOptions<LogRecordExportProcessorOptions>>().Value.BatchExportProcessorOptions;
        var multiplier = environment ? 2 : 1;
        batch.MaxQueueSize.ShouldEqual(256 * multiplier);
        batch.MaxExportBatchSize.ShouldEqual(64 * multiplier);
        batch.ScheduledDelayMilliseconds.ShouldEqual(250 * multiplier);
        batch.ExporterTimeoutMilliseconds.ShouldEqual(500 * multiplier);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    void should_honor_signal_specific_timeout_and_compression(bool environment)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318",
            ["OTEL_EXPORTER_OTLP_TIMEOUT"] = "1000",
            ["OTEL_EXPORTER_OTLP_COMPRESSION"] = "none",
            ["OTEL_EXPORTER_OTLP_LOGS_TIMEOUT"] = "2500",
            ["OTEL_EXPORTER_OTLP_LOGS_COMPRESSION"] = "gzip"
        }).Build();
        if (environment)
        {
            Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_LOGS_TIMEOUT", "3500");
            Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_LOGS_COMPRESSION", "none");
        }
        var exporter = new OtlpExporterOptions();
        typeof(OpenTelemetryExtensions).GetMethod("ConfigureExporter", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [exporter, configuration, "LOGS", "logs"]);
        exporter.TimeoutMilliseconds.ShouldEqual(environment ? 3500 : 2500);
        exporter.Compression.ShouldEqual(environment ? OtlpExportCompression.None : OtlpExportCompression.GZip);
    }
}
