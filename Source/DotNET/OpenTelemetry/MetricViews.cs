// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Composes metric shaping into a single SDK view.
/// </summary>
internal sealed class MetricViews
{
    readonly bool _enabled;
    readonly int _cardinalityLimit;
    readonly double[]? _secondsHistogramBoundaries;
    readonly Dictionary<string, MetricPolicy> _policies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricViews"/> class.
    /// Copies all metric settings before validating them, even when shaping is disabled.
    /// </summary>
    /// <param name="options">The caller-owned telemetry options.</param>
    /// <exception cref="InvalidMetricPolicy">A metric setting is invalid.</exception>
    internal MetricViews(CratisOpenTelemetryOptions options)
    {
        _enabled = options.EnableMetricViews;
        _cardinalityLimit = options.MetricCardinalityLimit;
        _secondsHistogramBoundaries = options.SecondsHistogramBoundaries is { } boundaries ? [.. boundaries] : null;
        foreach (var (name, policy) in options.MetricPolicies)
        {
            _policies[name] = new()
            {
                CardinalityLimit = policy.CardinalityLimit,
                Boundaries = policy.Boundaries is { } policyBoundaries ? [.. policyBoundaries] : null,
                TagKeys = policy.TagKeys is { } tagKeys ? [.. tagKeys] : null
            };
        }
        Validate();
    }

    /// <summary>
    /// Registers the owned view only when explicitly enabled at registration.
    /// </summary>
    /// <param name="builder">The metrics builder.</param>
    internal void AddTo(MeterProviderBuilder builder)
    {
        if (_enabled)
        {
            builder.AddView(Configure);
        }
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

    void Validate()
    {
        ValidateLimit(_cardinalityLimit, nameof(CratisOpenTelemetryOptions.MetricCardinalityLimit));
        ValidateBoundaries(_secondsHistogramBoundaries, nameof(CratisOpenTelemetryOptions.SecondsHistogramBoundaries));
        foreach (var (name, policy) in _policies)
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
    /// <returns>The composed configuration, or null for an unchanged stream.</returns>
    MetricStreamConfiguration? Configure(Instrument instrument)
    {
        _policies.TryGetValue(instrument.Name, out var policy);
        var limit = policy?.CardinalityLimit ?? _cardinalityLimit;
        var boundaries = HistogramBoundaries(instrument, policy);
        if (IsUnchanged(policy, boundaries, limit))
        {
            return null;
        }

        var configuration = boundaries is null
            ? new MetricStreamConfiguration()
            : new ExplicitBucketHistogramConfiguration { Boundaries = [.. boundaries] };
        configuration.TagKeys = policy?.TagKeys is { } tagKeys ? [.. tagKeys] : null;
        configuration.CardinalityLimit = limit;
        return configuration;
    }

    double[]? HistogramBoundaries(Instrument instrument, MetricPolicy? policy)
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
        return UsesSecondsBuckets(instrument) ? _secondsHistogramBoundaries : null;
    }
}
