// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Composes metric shaping into a single SDK view.
/// </summary>
internal static class MetricViews
{
    /// <summary>
    /// Validates metric settings before registering providers.
    /// </summary>
    /// <param name="options">The shared telemetry options.</param>
    /// <exception cref="InvalidMetricPolicy">A metric setting is invalid.</exception>
    internal static void Validate(CratisOpenTelemetryOptions options)
    {
        ValidateLimit(options.MetricCardinalityLimit, nameof(options.MetricCardinalityLimit));
        ValidateBoundaries(options.SecondsHistogramBoundaries, nameof(options.SecondsHistogramBoundaries));
        foreach (var (name, policy) in options.MetricPolicies)
        {
            var setting = $"MetricPolicies[\"{name}\"]";
            ValidateLimit(policy.CardinalityLimit, $"{setting}.CardinalityLimit");
            ValidateBoundaries(policy.Boundaries, $"{setting}.Boundaries");
            if (policy.TagKeys?.Any(string.IsNullOrWhiteSpace) == true)
            {
                throw new InvalidMetricPolicy($"{setting}.TagKeys", "must not contain null or whitespace keys.");
            }
        }
    }

    /// <summary>
    /// Configures a stream only when the shared setup changes its shaping.
    /// </summary>
    /// <param name="instrument">The instrument being subscribed to.</param>
    /// <param name="options">The shared telemetry options.</param>
    /// <returns>The composed configuration, or null for an unchanged stream.</returns>
    internal static MetricStreamConfiguration? Configure(Instrument instrument, CratisOpenTelemetryOptions options)
    {
        options.MetricPolicies.TryGetValue(instrument.Name, out var policy);
        var limit = policy?.CardinalityLimit ?? options.MetricCardinalityLimit;
        var boundaries = HistogramBoundaries(instrument, policy, options);
        if (IsUnchanged(policy, boundaries, limit))
        {
            return null;
        }

        var configuration = boundaries is null
            ? new MetricStreamConfiguration()
            : new ExplicitBucketHistogramConfiguration { Boundaries = boundaries };
        configuration.TagKeys = policy?.TagKeys;
        configuration.CardinalityLimit = limit;
        return configuration;
    }

    static double[]? HistogramBoundaries(Instrument instrument, MetricPolicy? policy, CratisOpenTelemetryOptions options)
    {
        if (!IsHistogram(instrument))
        {
            return null;
        }
        if (policy?.Boundaries is not null)
        {
            return policy.Boundaries;
        }
        if (HasAdvisedBoundaries(instrument))
        {
            return null;
        }
        return UsesSecondsBuckets(instrument) ? options.SecondsHistogramBoundaries : null;
    }

    static bool IsHistogram(Instrument instrument) => instrument.GetType().IsGenericType && instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>);

    static bool UsesSecondsBuckets(Instrument instrument) => instrument.Meter.Name.StartsWith("Cratis.", StringComparison.OrdinalIgnoreCase) && instrument.Unit == "s";

    static bool HasAdvisedBoundaries(Instrument instrument) => instrument switch
    {
        Histogram<double> histogram => HasBoundaries(histogram),
        Histogram<float> histogram => HasBoundaries(histogram),
        Histogram<decimal> histogram => HasBoundaries(histogram),
        _ => HasAdvisedIntegerBoundaries(instrument)
    };

    static bool HasAdvisedIntegerBoundaries(Instrument instrument) => instrument switch
    {
        Histogram<long> histogram => HasBoundaries(histogram),
        Histogram<int> histogram => HasBoundaries(histogram),
        Histogram<short> histogram => HasBoundaries(histogram),
        Histogram<byte> histogram => HasBoundaries(histogram),
        _ => false
    };

    static bool HasBoundaries<T>(Histogram<T> histogram)
        where T : struct => histogram.Advice?.HistogramBucketBoundaries is not null;

    static bool IsUnchanged(MetricPolicy? policy, double[]? boundaries, int limit) =>
        policy is null && boundaries is null && limit == CratisOpenTelemetryOptions.DefaultMetricCardinalityLimit;

    static void ValidateLimit(int? limit, string setting)
    {
        if (limit <= 0)
        {
            throw new InvalidMetricPolicy(setting, "must be positive.");
        }
    }

    static void ValidateBoundaries(double[]? boundaries, string setting)
    {
        if (boundaries is null) return;
        if (!Array.TrueForAll(boundaries, double.IsFinite))
        {
            throw new InvalidMetricPolicy(setting, "must contain finite, strictly ascending boundaries.");
        }
        if (boundaries.Zip(boundaries.Skip(1)).Any(pair => pair.First >= pair.Second))
        {
            throw new InvalidMetricPolicy(setting, "must contain finite, strictly ascending boundaries.");
        }
    }
}
