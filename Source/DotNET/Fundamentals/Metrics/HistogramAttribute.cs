// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics;

/// <summary>
/// Marks a method that records a distribution of measurements.
/// </summary>
/// <typeparam name="T">The numeric measurement type.</typeparam>
/// <param name="name">Name of the histogram.</param>
/// <param name="description">Description of the histogram.</param>
/// <param name="unit">UCUM unit of the histogram, such as s for seconds.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class HistogramAttribute<T>(string name, string description, string? unit = null) : Attribute
{
    /// <summary>
    /// Gets the name of the histogram.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the description of the histogram.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Gets the UCUM unit of the histogram.
    /// </summary>
    public string? Unit { get; } = unit;
}
