// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from './geospatial/Point';

/**
 * @deprecated Use Point from './geospatial' instead. This alias will be removed in a future version.
 * Represents a geographic coordinate with longitude and latitude.
 */
export class Coordinate extends Point {
    /**
     * Converts the Coordinate to a JSON representation (backwards compatible format).
     * @returns {object} JSON representation with longitude and latitude (not GeoJSON).
     */
    toJSON(): object {
        return {
            longitude: this.longitude,
            latitude: this.latitude
        };
    }
}
