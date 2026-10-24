// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Represents helpers for configuring context propagation.
/// </summary>
static class OpenTelemetryPropagation
{
    /// <summary>
    /// Sets the default text map propagator from a comma-separated list of propagator names.
    /// </summary>
    /// <param name="configured">The configured names; preserves the current propagator when null. "none" disables propagation.</param>
    internal static void Configure(string? configured)
    {
        if (configured is null)
        {
            return;
        }

        var names = configured.Split(',').Select(name => name.Trim().ToLowerInvariant()).Distinct().ToArray();
        var propagators = names.Contains("none") ? [] : names.Select(Create).OfType<TextMapPropagator>().ToArray();
        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(propagators));
    }

    static TextMapPropagator? Create(string name) => name switch
    {
        "tracecontext" => new TraceContextPropagator(),
        "baggage" => new BaggagePropagator(),
        _ => null
    };
}
