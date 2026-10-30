// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
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
    /// The SDK's default cardinality limit per metric stream.
    /// </summary>
    public const int DefaultMetricCardinalityLimit = 2000;

    /// <summary>
    /// Gets or sets the positive cardinality limit per stream, unless overridden by a metric policy.
    /// Defaults to the SDK's existing limit of 2000.
    /// </summary>
    public int MetricCardinalityLimit { get; set; } = DefaultMetricCardinalityLimit;

    /// <summary>
    /// Gets or sets second-based buckets for Cratis.* histograms with unit s and no instrument advice.
    /// Metric policy boundaries take precedence over advice, which takes precedence over these buckets.
    /// Null retains instrument advice or SDK defaults instead of automatically supplying seconds buckets.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Mutable configuration mirrors the SDK histogram boundaries.")]
    public double[]? SecondsHistogramBoundaries { get; set; } = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10];

    /// <summary>
    /// Gets policies keyed by exact, case-insensitive instrument name, without wildcards or meter qualifiers.
    /// Policy boundaries override advice and default buckets; policy limits override the default limit.
    /// </summary>
    public IDictionary<string, MetricPolicy> MetricPolicies { get; } = new Dictionary<string, MetricPolicy>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the fallback service name when standard configuration does not name the service.
    /// </summary>
    public string ServiceName { get; set; } = (Assembly.GetEntryAssembly()?.GetName().Name ?? "cratis-application").ToLowerInvariant();

    /// <summary>
    /// Gets or sets the application version, defaulting to its assembly informational version.
    /// </summary>
    public string? ServiceVersion { get; set; } = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    /// <summary>
    /// Gets or sets a value indicating whether exception messages and stack traces are included in exported logs.
    /// Defaults to false so only the exception type is exported.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>
    /// Gets or sets additional tracing instrumentation, such as gRPC client, MongoDB or Orleans sources.
    /// Standard OTEL sampling settings take precedence over samplers selected here.
    /// </summary>
    public Action<TracerProviderBuilder>? ConfigureTracing { get; set; }

    /// <summary>
    /// Gets or sets additional metric instrumentation.
    /// </summary>
    public Action<MeterProviderBuilder>? ConfigureMetrics { get; set; }
}
