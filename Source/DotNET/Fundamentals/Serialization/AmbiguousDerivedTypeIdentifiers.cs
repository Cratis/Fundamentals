// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Serialization;

/// <summary>
/// Exception that gets thrown when multiple types have the same derived type identifier.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AmbiguousDerivedTypeIdentifiers"/> class.
/// </remarks>
/// <param name="types">Types that have the same identifier.</param>
public class AmbiguousDerivedTypeIdentifiers(IEnumerable<Type> types) : Exception($"The types '{string.Join(", ", types.Select(_ => _.FullName))}' have the same derived type identifier.")
{
}
