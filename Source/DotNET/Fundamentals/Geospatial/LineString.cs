// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Geospatial;

/// <summary>
/// Represents a line string composed of two or more points.
/// </summary>
/// <param name="Coordinates">The points that make up the line string.</param>
/// <remarks>
/// Initializes a new instance of <see cref="LineString"/>.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "GeoJSON spec requires array representation")]
public record LineString(Point[] Coordinates);
