// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using OpenTelemetry.Exporter;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Represents helpers for applying OTLP exporter settings.
/// </summary>
static class OpenTelemetryExporterSettings
{
    /// <summary>
    /// Applies a timeout and compression setting to the exporter options when valid.
    /// </summary>
    /// <param name="exporter">The OTLP exporter options to update.</param>
    /// <param name="timeout">The timeout in milliseconds, ignored if not a positive integer.</param>
    /// <param name="compression">The compression name, either "gzip" or "none"; other values leave the current setting.</param>
    internal static void Apply(OtlpExporterOptions exporter, string? timeout, string? compression)
    {
        if (int.TryParse(timeout, out var milliseconds) && milliseconds > 0)
        {
            exporter.TimeoutMilliseconds = milliseconds;
        }
        exporter.Compression = compression?.Trim().ToLowerInvariant() switch
        {
            "gzip" => OtlpExportCompression.GZip,
            "none" => OtlpExportCompression.None,
            _ => exporter.Compression
        };
    }
}
