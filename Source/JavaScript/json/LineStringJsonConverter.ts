// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { Point, LineString } from '../geospatial';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for LineString type (GeoJSON format).
 */
export class LineStringJsonConverter extends JsonConverter<LineString> {
    /** @inheritdoc */
    get type(): Constructor<LineString> {
        return LineString;
    }

    /** @inheritdoc */
    read(value: any): LineString {
        if (value === null || value === undefined) {
            throw new Error('Cannot deserialize null or undefined to LineString');
        }
        if (value.type !== 'LineString' || !value.coordinates || value.coordinates.length < 2) {
            throw new Error('Cannot deserialize LineString: invalid GeoJSON format');
        }
        const points = value.coordinates.map((coord: number[]) => new Point(coord[0], coord[1]));
        return new LineString(points);
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toJSON() ?? null;
    }
}
