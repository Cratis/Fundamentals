// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces;

/// <summary>
/// Attribute for marking a method as a span for the traces code generator.
/// </summary>
/// <param name="name">Name of the span.</param>
/// <param name="kind">The <see cref="System.Diagnostics.ActivityKind"/> of the span.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SpanAttribute(string name, System.Diagnostics.ActivityKind kind = System.Diagnostics.ActivityKind.Internal) : Attribute
{
    /// <summary>
    /// Gets the name of the span.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the kind of the span.
    /// </summary>
    public System.Diagnostics.ActivityKind Kind { get; } = kind;
}
