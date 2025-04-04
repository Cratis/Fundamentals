// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Execution;

/// <summary>
/// Represents an identifier for correlation.
/// </summary>
/// <param name="Value">Actual value.</param>
public record CorrelationId(Guid Value) : ConceptAs<Guid>(Value)
{
    /// <summary>
    /// Gets the value for not set.
    /// </summary>
    public static readonly CorrelationId NotSet = Guid.Empty;

    /// <summary>
    /// Implicitly convert from <see cref="string"/> to <see cref="CorrelationId"/>.
    /// </summary>
    /// <param name="id"><see cref="Guid"/> to convert from.</param>
    /// <returns>A new <see cref="CorrelationId"/>.</returns>
    public static implicit operator CorrelationId(Guid id) => new(id);

    /// <summary>
    /// Create a new <see cref="CorrelationId"/> based on a new <see cref="Guid"/>.
    /// </summary>
    /// <returns>A new <see cref="CorrelationId"/>.</returns>
    public static CorrelationId New() => new(Guid.NewGuid());
}