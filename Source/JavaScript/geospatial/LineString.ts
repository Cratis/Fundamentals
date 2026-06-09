// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from './Point';

/**
 * Represents a line string composed of two or more points.
 */
export class LineString {
    coordinates!: Point[];

    /**
     * Creates a new LineString instance.
     * @param {Point[]} coordinates - The points that make up the line string (minimum 2 points).
     */
    constructor(coordinates: Point[]) {
        if (coordinates.length < 2) {
            throw new Error('LineString must have at least 2 points');
        }
        this.coordinates = coordinates;
    }

    /**
     * Converts the LineString to a GeoJSON representation.
     * @returns {object} GeoJSON representation.
     */
    toJSON(): object {
        return {
            type: 'LineString',
            coordinates: this.coordinates.map(p => [p.longitude, p.latitude])
        };
    }
}
