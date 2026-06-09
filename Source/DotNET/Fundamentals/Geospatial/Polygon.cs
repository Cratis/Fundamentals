// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Geospatial;

#pragma warning disable CA1819 // Properties should not return arrays

/// <summary>
/// Represents a polygon with an outer shell and optional holes.
/// </summary>
/// <param name="Shell">The outer boundary of the polygon.</param>
/// <param name="Holes">Optional inner boundaries (holes) within the polygon.</param>
/// <remarks>
/// Initializes a new instance of <see cref="Polygon"/>.
/// </remarks>
public record Polygon(LinearRing Shell, LinearRing[] Holes);
