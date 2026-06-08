// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Geospatial;

/// <summary>
/// Represents a geographic coordinate with longitude and latitude.
/// </summary>
/// <param name="Longitude">The longitude of the coordinate.</param>
/// <param name="Latitude">The latitude of the coordinate.</param>
public record Coordinate(double Longitude, double Latitude);
