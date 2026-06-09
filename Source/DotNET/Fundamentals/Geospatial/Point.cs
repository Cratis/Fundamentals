// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Geospatial;

/// <summary>
/// Represents a geographic point with longitude and latitude.
/// </summary>
/// <param name="Longitude">The longitude of the point.</param>
/// <param name="Latitude">The latitude of the point.</param>
public record Point(double Longitude, double Latitude);
