// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Geospatial;

#pragma warning disable CA1819 // Properties should not return arrays

/// <summary>
/// Represents a closed linear ring (polygon boundary).
/// </summary>
/// <param name="Coordinates">The points that make up the linear ring. First and last point must be identical to close the ring.</param>
/// <remarks>
/// Initializes a new instance of <see cref="LinearRing"/>.
/// </remarks>
public record LinearRing(Point[] Coordinates);
