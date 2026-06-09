// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../geospatial/Point';
import { LineString } from '../geospatial/LineString';
import { LineStringJsonConverter } from '../json/LineStringJsonConverter';

describe('when writing linestring with linestring converter', () => {
    const converter = new LineStringJsonConverter();
    const lineString = new LineString([
        new Point(10.5, 20.3),
        new Point(11.2, 21.1)
    ]);
    const result = converter.write(lineString);

    it('should have type property', () => result.type.should.equal('LineString'));
    it('should have coordinates array', () => result.coordinates.should.be.instanceof(Array));
    it('should have two points', () => result.coordinates.length.should.equal(2));
    it('should have first point coordinates', () => {
        result.coordinates[0][0].should.equal(10.5);
        result.coordinates[0][1].should.equal(20.3);
    });
    it('should have second point coordinates', () => {
        result.coordinates[1][0].should.equal(11.2);
        result.coordinates[1][1].should.equal(21.1);
    });
});
