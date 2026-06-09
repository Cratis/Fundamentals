// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from '../fieldDecorator';

/**
 * Represents a geographic point with longitude and latitude.
 */
export class Point {
    @field(Number)
    longitude!: number;

    @field(Number)
    latitude!: number;

    /**
     * Creates a new Point instance.
     * @param {number} longitude - The longitude of the point.
     * @param {number} latitude - The latitude of the point.
     */
    constructor(longitude: number, latitude: number) {
        this.longitude = longitude;
        this.latitude = latitude;
    }

    /**
     * Converts the Point to a GeoJSON representation.
     * @returns {object} GeoJSON representation.
     */
    toJSON(): object {
        return {
            type: 'Point',
            coordinates: [this.longitude, this.latitude]
        };
    }
}
