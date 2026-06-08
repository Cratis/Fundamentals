// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from './fieldDecorator';

/**
 * Represents a geographic coordinate with longitude and latitude.
 */
export class Coordinate {
    @field(Number)
    longitude!: number;

    @field(Number)
    latitude!: number;

    /**
     * Creates a new Coordinate instance.
     * @param {number} longitude - The longitude of the coordinate.
     * @param {number} latitude - The latitude of the coordinate.
     */
    constructor(longitude?: number, latitude?: number) {
        if (longitude !== undefined && latitude !== undefined) {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }

    /**
     * Converts the Coordinate to a JSON representation.
     * @returns {object} JSON representation with longitude and latitude.
     */
    toJSON(): object {
        return {
            longitude: this.longitude,
            latitude: this.latitude
        };
    }
}
