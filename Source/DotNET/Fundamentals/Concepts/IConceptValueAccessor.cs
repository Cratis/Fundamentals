// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Internal non-generic access to the value held by a <see cref="ConceptAs{T}"/>, avoiding reflection on hot paths.
/// </summary>
internal interface IConceptValueAccessor
{
    /// <summary>
    /// Gets the underlying value, boxed.
    /// </summary>
    object ValueAsObject { get; }
}
