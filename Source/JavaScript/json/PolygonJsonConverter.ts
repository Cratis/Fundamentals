// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { Point, LinearRing, Polygon } from '../geospatial';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for Polygon type (GeoJSON format).
 */
export class PolygonJsonConverter extends JsonConverter<Polygon> {
    /** @inheritdoc */
    get type(): Constructor<Polygon> {
        return Polygon;
    }

    /** @inheritdoc */
    read(value: any): Polygon {
        if (value === null || value === undefined) {
            throw new Error('Cannot deserialize null or undefined to Polygon');
        }
        if (value.type !== 'Polygon' || !value.coordinates || value.coordinates.length === 0) {
            throw new Error('Cannot deserialize Polygon: invalid GeoJSON format');
        }
        const shell = new LinearRing(value.coordinates[0].map((coord: number[]) => new Point(coord[0], coord[1])));
        const holes = value.coordinates.slice(1).map((ring: number[][]) => 
            new LinearRing(ring.map((coord: number[]) => new Point(coord[0], coord[1])))
        );
        return new Polygon(shell, holes);
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toJSON() ?? null;
    }
}
