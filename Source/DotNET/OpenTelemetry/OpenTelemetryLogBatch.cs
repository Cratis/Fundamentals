// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Represents helpers for configuring and creating batch log record processors.
/// </summary>
static class OpenTelemetryLogBatch
{
    /// <summary>
    /// Configures batch options from the OTEL_BLRP_* settings, keeping current values when a setting is missing or not positive.
    /// The export batch size never exceeds the queue size, as the batch processor requires.
    /// </summary>
    /// <param name="options">The batch options to update.</param>
    /// <param name="read">A function that reads a setting by name.</param>
    internal static void Configure(BatchExportLogRecordProcessorOptions options, Func<string, string?> read)
    {
        options.MaxQueueSize = ReadPositive(read("OTEL_BLRP_MAX_QUEUE_SIZE"), options.MaxQueueSize);
        options.MaxExportBatchSize = Math.Min(ReadPositive(read("OTEL_BLRP_MAX_EXPORT_BATCH_SIZE"), options.MaxExportBatchSize), options.MaxQueueSize);
        options.ScheduledDelayMilliseconds = ReadPositive(read("OTEL_BLRP_SCHEDULE_DELAY"), options.ScheduledDelayMilliseconds);
        options.ExporterTimeoutMilliseconds = ReadPositive(read("OTEL_BLRP_EXPORT_TIMEOUT"), options.ExporterTimeoutMilliseconds);
    }

    /// <summary>
    /// Creates a batch processor for the exporter using the given options.
    /// </summary>
    /// <param name="exporter">The OTLP log exporter.</param>
    /// <param name="options">The batch options supplying queue size, delay, timeout and batch size.</param>
    /// <returns>The batch log record export processor.</returns>
    internal static BatchLogRecordExportProcessor Create(OtlpLogExporter exporter, BatchExportLogRecordProcessorOptions options) =>
        new(exporter, options.MaxQueueSize, options.ScheduledDelayMilliseconds, options.ExporterTimeoutMilliseconds, options.MaxExportBatchSize);

    static int ReadPositive(string? value, int fallback) => int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}
