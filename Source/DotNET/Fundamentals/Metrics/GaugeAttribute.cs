// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics;

/// <summary>
/// Attribute for marking a method as a gauge for the metrics code generator.
/// </summary>
/// <typeparam name="T">Type of counter.</typeparam>
/// <remarks>
/// Gauges are used to measure the value of a particular thing at a particular time.
/// </remarks>
/// <param name="name">Name of the gauge.</param>
/// <param name="description">Description of the counter.</param>
/// <param name="unit">UCUM unit of the gauge.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GaugeAttribute<T>(string name, string description, string? unit) : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GaugeAttribute{T}"/> class without a unit.
    /// </summary>
    /// <param name="name">Name of the gauge.</param>
    /// <param name="description">Description of the gauge.</param>
    public GaugeAttribute(string name, string description) : this(name, description, null)
    {
    }

    /// <summary>
    /// Gets the UCUM unit of the gauge.
    /// </summary>
    public string? Unit { get; } = unit;

    /// <summary>
    /// Gets the name of the counter.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the description of the counter.
    /// </summary>
    public string Description { get; } = description;
}
