// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from './Point';

/**
 * Represents a closed linear ring (polygon boundary).
 */
export class LinearRing {
    coordinates!: Point[];

    /**
     * Creates a new LinearRing instance.
     * @param {Point[]} coordinates - The points that make up the linear ring.
     */
    constructor(coordinates?: Point[]) {
        if (coordinates !== undefined) {
            this.coordinates = coordinates;
        }
    }

    /**
     * Converts the LinearRing to a GeoJSON coordinate array.
     * @returns {number[][]} GeoJSON coordinate array.
     */
    toJSON(): number[][] {
        return this.coordinates.map(p => [p.longitude, p.latitude]);
    }
}
