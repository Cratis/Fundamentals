// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Serialization;

/// <summary>
/// The unique identifier identifying a derived type.
/// </summary>
/// <param name="id">The arbitrary string that identifies the type uniquely.</param>
public record DerivedTypeId(string id) : ConceptAs<string>(id)
{
    /// <summary>
    /// Implicitly convert from a string to <see cref="DerivedTypeId"/>.
    /// </summary>
    /// <param name="id">String to convert from.</param>
    public static implicit operator DerivedTypeId(string id) => new(id);

    /// <summary>
    /// Implicitly convert from <see cref="DerivedTypeId"/> to string.
    /// </summary>
    /// <param name="id"><see cref="DerivedTypeId"/> to convert from.</param>
    public static implicit operator string(DerivedTypeId id) => id.Value;
}
