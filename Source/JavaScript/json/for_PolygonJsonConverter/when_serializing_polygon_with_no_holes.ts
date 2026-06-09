// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../../geospatial/Point';
import { LinearRing } from '../../geospatial/LinearRing';
import { Polygon } from '../../geospatial/Polygon';
import { JsonSerializer } from '../../JsonSerializer';

describe('when serializing polygon with no holes', () => {
    const shell = new LinearRing([
        new Point(0, 0),
        new Point(10, 0),
        new Point(10, 10),
        new Point(0, 10),
        new Point(0, 0)
    ]);
    const polygon = new Polygon(shell, []);
    const result = JsonSerializer.serialize(polygon);
    const deserialized = JSON.parse(result);

    it('should have type of Polygon', () => deserialized.type.should.equal('Polygon'));
    it('should have coordinates array', () => deserialized.coordinates.should.be.an('array'));
    it('should have one ring (shell only)', () => deserialized.coordinates.length.should.equal(1));
    it('should have five points in shell', () => deserialized.coordinates[0].length.should.equal(5));
    it('should have correct first point', () => {
        deserialized.coordinates[0][0][0].should.equal(0);
        deserialized.coordinates[0][0][1].should.equal(0);
    });
    it('should have correct last point (closed ring)', () => {
        deserialized.coordinates[0][4][0].should.equal(0);
        deserialized.coordinates[0][4][1].should.equal(0);
    });
});
