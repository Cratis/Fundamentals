// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../geospatial/Point';
import { LinearRing } from '../geospatial/LinearRing';
import { Polygon } from '../geospatial/Polygon';
import { JsonSerializer } from '../JsonSerializer';

describe('when serializing polygon with holes', () => {
    const shell = new LinearRing([
        new Point(0, 0),
        new Point(20, 0),
        new Point(20, 20),
        new Point(0, 20),
        new Point(0, 0)
    ]);
    const hole1 = new LinearRing([
        new Point(2, 2),
        new Point(8, 2),
        new Point(8, 8),
        new Point(2, 8),
        new Point(2, 2)
    ]);
    const hole2 = new LinearRing([
        new Point(12, 12),
        new Point(18, 12),
        new Point(18, 18),
        new Point(12, 18),
        new Point(12, 12)
    ]);
    const polygon = new Polygon(shell, [hole1, hole2]);
    const result = JsonSerializer.serialize(polygon);
    const deserialized = JSON.parse(result);

    it('should have type of Polygon', () => deserialized.type.should.equal('Polygon'));
    it('should have coordinates array', () => deserialized.coordinates.should.be.an('array'));
    it('should have three rings (shell + 2 holes)', () => deserialized.coordinates.length.should.equal(3));
    it('should have five points in shell', () => deserialized.coordinates[0].length.should.equal(5));
    it('should have five points in first hole', () => deserialized.coordinates[1].length.should.equal(5));
    it('should have five points in second hole', () => deserialized.coordinates[2].length.should.equal(5));
    it('should have correct first point in shell', () => {
        deserialized.coordinates[0][0][0].should.equal(0);
        deserialized.coordinates[0][0][1].should.equal(0);
    });
    it('should have correct first point in first hole', () => {
        deserialized.coordinates[1][0][0].should.equal(2);
        deserialized.coordinates[1][0][1].should.equal(2);
    });
    it('should have correct first point in second hole', () => {
        deserialized.coordinates[2][0][0].should.equal(12);
        deserialized.coordinates[2][0][1].should.equal(12);
    });
});
