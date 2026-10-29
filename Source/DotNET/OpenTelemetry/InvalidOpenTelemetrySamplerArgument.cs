// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry;

/// <summary>
/// Exception thrown when a ratio sampler has no valid sampling probability.
/// </summary>
/// <param name="value">The configured sampling probability, or null when missing.</param>
public class InvalidOpenTelemetrySamplerArgument(string? value)
    : Exception($"OTEL_TRACES_SAMPLER_ARG must be a number between 0 and 1 for a ratio sampler; value: '{value ?? "<missing>"}'.");
