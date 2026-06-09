// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { LinearRing } from './LinearRing';

/**
 * Represents a polygon with an outer shell and optional holes.
 */
export class Polygon {
    shell!: LinearRing;
    holes!: LinearRing[];

    /**
     * Creates a new Polygon instance.
     * @param {LinearRing} shell - The outer boundary of the polygon.
     * @param {LinearRing[]} holes - Optional inner boundaries (holes) within the polygon.
     */
    constructor(shell: LinearRing, holes?: LinearRing[]) {
        this.shell = shell;
        this.holes = holes || [];
    }

    /**
     * Converts the Polygon to a GeoJSON representation.
     * @returns {object} GeoJSON representation.
     */
    toJSON(): object {
        const rings = [this.shell.toJSON()];
        if (this.holes && this.holes.length > 0) {
            rings.push(...this.holes.map(h => h.toJSON()));
        }
        return {
            type: 'Polygon',
            coordinates: rings
        };
    }
}
