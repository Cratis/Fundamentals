// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { Coordinate } from '../Coordinate';
import { JsonConverter } from './JsonConverter';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * JSON converter for Coordinate type (deprecated, uses legacy format).
 */
export class CoordinateJsonConverter extends JsonConverter<Coordinate> {
    /** @inheritdoc */
    get type(): Constructor<Coordinate> {
        return Coordinate;
    }

    /** @inheritdoc */
    read(value: any): Coordinate {
        if (value === null || value === undefined) {
            throw new Error('Cannot deserialize null or undefined to Coordinate');
        }
        if (value.longitude === undefined || value.latitude === undefined) {
            throw new Error('Cannot deserialize Coordinate: longitude and latitude are required');
        }
        const coordinate = new Coordinate(value.longitude, value.latitude);
        return coordinate;
    }

    /** @inheritdoc */
    write(value: any): any {
        return value?.toJSON() ?? null;
    }
}
