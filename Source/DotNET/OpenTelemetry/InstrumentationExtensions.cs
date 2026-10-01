// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Subscribes providers to all Cratis instrumentation scopes.
/// </summary>
public static class InstrumentationExtensions
{
    /// <summary>
    /// The wildcard shared by Cratis activity sources and meters.
    /// </summary>
    public const string InstrumentationScope = "Cratis.*";

    /// <summary>
    /// Subscribes to Cratis activity sources, including sources created later.
    /// </summary>
    /// <param name="builder">The tracing provider builder.</param>
    /// <returns>The builder for continuation.</returns>
    public static TracerProviderBuilder AddCratisInstrumentation(this TracerProviderBuilder builder) => builder.AddSource(InstrumentationScope);

    /// <summary>
    /// Subscribes to Cratis meters, including meters created later.
    /// </summary>
    /// <param name="builder">The metric provider builder.</param>
    /// <returns>The builder for continuation.</returns>
    public static MeterProviderBuilder AddCratisInstrumentation(this MeterProviderBuilder builder) => builder.AddMeter(InstrumentationScope);
}
