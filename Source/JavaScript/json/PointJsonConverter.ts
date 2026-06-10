// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { Point } from '../geospatial';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for Point type (GeoJSON format).
 */
export class PointJsonConverter extends JsonConverter<Point> {
    /** @inheritdoc */
    get type(): Constructor<Point> {
        return Point;
    }

    /** @inheritdoc */
    read(value: any): Point {
        if (value === null || value === undefined) {
            throw new Error('Cannot deserialize null or undefined to Point');
        }
        if (value.type === 'Point' && value.coordinates && value.coordinates.length === 2) {
            return new Point(value.coordinates[0], value.coordinates[1]);
        }
        if (typeof value.longitude === 'number' && typeof value.latitude === 'number') {
            return new Point(value.longitude, value.latitude);
        }
        throw new Error('Cannot deserialize Point: expected GeoJSON format { type, coordinates } or { longitude, latitude }');
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toJSON() ?? null;
    }
}
