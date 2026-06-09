// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../geospatial/Point';
import { LinearRing } from '../geospatial/LinearRing';
import { Polygon } from '../geospatial/Polygon';
import { PolygonJsonConverter } from '../json/PolygonJsonConverter';

describe('when writing polygon with polygon converter', () => {
    const converter = new PolygonJsonConverter();
    const polygon = new Polygon(
        new LinearRing([
            new Point(0, 0),
            new Point(10, 0),
            new Point(10, 10),
            new Point(0, 10),
            new Point(0, 0)
        ]),
        []
    );
    const result = converter.write(polygon);

    it('should have type property', () => result.type.should.equal('Polygon'));
    it('should have coordinates array', () => result.coordinates.should.be.instanceof(Array));
    it('should have one ring', () => result.coordinates.length.should.equal(1));
    it('should have five points in ring', () => result.coordinates[0].length.should.equal(5));
    it('should have first point coordinates', () => {
        result.coordinates[0][0][0].should.equal(0);
        result.coordinates[0][0][1].should.equal(0);
    });
    it('should have closed ring', () => {
        result.coordinates[0][0][0].should.equal(result.coordinates[0][4][0]);
        result.coordinates[0][0][1].should.equal(result.coordinates[0][4][1]);
    });
});
