// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Diagnostics;

/// <summary>
/// Specifies the attribute key for a generated span or metric parameter.
/// </summary>
/// <param name="name">The telemetry attribute key.</param>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class TagAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the telemetry attribute key.
    /// </summary>
    public string Name { get; } = name;
}
