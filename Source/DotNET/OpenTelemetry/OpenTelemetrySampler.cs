// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Represents helpers for creating trace samplers from configuration.
/// </summary>
static class OpenTelemetrySampler
{
    /// <summary>
    /// Creates a sampler from its configured name, optionally wrapped as parent-based.
    /// </summary>
    /// <param name="name">The sampler name, such as "always_on", "always_off", "traceidratio" or a "parentbased_" variant.</param>
    /// <param name="argument">The sampler argument, used as the ratio for "traceidratio".</param>
    /// <returns>The sampler, or <see langword="null"/> if the name is not recognized.</returns>
    internal static Sampler? Create(string? name, string? argument)
    {
        var normalized = name?.Trim().ToLowerInvariant();
        var parentBased = normalized?.StartsWith("parentbased_", StringComparison.Ordinal) ?? false;
        var root = CreateRoot(parentBased ? normalized!["parentbased_".Length..] : normalized, argument);
        return parentBased && root is not null ? new ParentBasedSampler(root) : root;
    }

    static Sampler? CreateRoot(string? name, string? argument) => name switch
    {
        "always_on" => new AlwaysOnSampler(),
        "always_off" => new AlwaysOffSampler(),
        "traceidratio" => RatioSampler(argument),
        _ => null
    };

    static TraceIdRatioBasedSampler RatioSampler(string? argument)
    {
        if (!double.TryParse(argument, CultureInfo.InvariantCulture, out var ratio) || ratio is not (>= 0 and <= 1))
        {
            throw new InvalidOpenTelemetrySamplerArgument(argument);
        }
        return new TraceIdRatioBasedSampler(ratio);
    }
}
