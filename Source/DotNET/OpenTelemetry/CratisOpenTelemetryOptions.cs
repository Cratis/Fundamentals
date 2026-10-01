// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Application defaults and optional instrumentation for Cratis telemetry.
/// Standard OTEL configuration takes precedence over resource defaults.
/// </summary>
public class CratisOpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets the fallback service name when standard configuration does not name the service.
    /// </summary>
    public string ServiceName { get; set; } = (Assembly.GetEntryAssembly()?.GetName().Name ?? "cratis-application").ToLowerInvariant();

    /// <summary>
    /// Gets or sets the application version, defaulting to its assembly informational version.
    /// </summary>
    public string? ServiceVersion { get; set; } = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    /// <summary>
    /// Gets or sets additional tracing instrumentation, such as gRPC client, MongoDB or Orleans sources.
    /// </summary>
    public Action<TracerProviderBuilder>? ConfigureTracing { get; set; }

    /// <summary>
    /// Gets or sets additional metric instrumentation.
    /// </summary>
    public Action<MeterProviderBuilder>? ConfigureMetrics { get; set; }
}
