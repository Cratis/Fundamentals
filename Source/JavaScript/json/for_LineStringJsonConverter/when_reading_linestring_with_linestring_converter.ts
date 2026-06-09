// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { LineString } from '../geospatial/LineString';
import { LineStringJsonConverter } from '../json/LineStringJsonConverter';

describe('when reading linestring with linestring converter', () => {
    const converter = new LineStringJsonConverter();
    const result = converter.read({
        type: 'LineString',
        coordinates: [[10.5, 20.3], [11.2, 21.1]]
    });

    it('should return LineString instance', () => result.should.be.instanceof(LineString));
    it('should have two points', () => result.coordinates.length.should.equal(2));
    it('should have first point with correct coordinates', () => {
        result.coordinates[0].longitude.should.equal(10.5);
        result.coordinates[0].latitude.should.equal(20.3);
    });
    it('should have second point with correct coordinates', () => {
        result.coordinates[1].longitude.should.equal(11.2);
        result.coordinates[1].latitude.should.equal(21.1);
    });
});
