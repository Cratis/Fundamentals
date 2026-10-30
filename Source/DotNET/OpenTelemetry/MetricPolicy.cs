// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Overrides shaping for an instrument name in the composed metric view.
/// </summary>
public class MetricPolicy
{
    /// <summary>
    /// Gets or sets allowed tag keys. Null keeps all tags; an empty array removes all tags.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Mutable configuration mirrors the SDK metric stream options.")]
    public string[]? TagKeys { get; set; }

    /// <summary>
    /// Gets or sets histogram boundaries, overriding instrument advice and the default buckets.
    /// Null uses the default; ignored for non-histogram instruments.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Mutable configuration mirrors the SDK histogram boundaries.")]
    public double[]? Boundaries { get; set; }

    /// <summary>
    /// Gets or sets the positive cardinality limit. Null uses the configured default limit.
    /// </summary>
    public int? CardinalityLimit { get; set; }
}
