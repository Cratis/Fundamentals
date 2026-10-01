// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics;

/// <summary>
/// Attribute for marking a method as a counter for the metrics code generator.
/// </summary>
/// <typeparam name="T">Type of counter.</typeparam>
/// <remarks>
/// Counter is an Instrument which supports non-negative increments.
/// </remarks>
/// <param name="name">Name of the counter.</param>
/// <param name="description">Description of the counter.</param>
/// <param name="unit">UCUM unit of the counter.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class CounterAttribute<T>(string name, string description, string? unit) : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CounterAttribute{T}"/> class without a unit.
    /// </summary>
    /// <param name="name">Name of the counter.</param>
    /// <param name="description">Description of the counter.</param>
    public CounterAttribute(string name, string description) : this(name, description, null)
    {
    }

    /// <summary>
    /// Gets the UCUM unit of the counter.
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
