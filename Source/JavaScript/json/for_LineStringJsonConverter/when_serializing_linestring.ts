// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../geospatial/Point';
import { LineString } from '../geospatial/LineString';
import { JsonSerializer } from '../JsonSerializer';

describe('when serializing linestring', () => {
    const point1 = new Point(10.5, 20.3);
    const point2 = new Point(30.1, 40.2);
    const point3 = new Point(50.7, 60.8);
    const lineString = new LineString([point1, point2, point3]);
    const result = JsonSerializer.serialize(lineString);
    const deserialized = JSON.parse(result);

    it('should have type of LineString', () => deserialized.type.should.equal('LineString'));
    it('should have coordinates array', () => deserialized.coordinates.should.be.an('array'));
    it('should have three coordinate pairs', () => deserialized.coordinates.length.should.equal(3));
    it('should have correct first point longitude', () => deserialized.coordinates[0][0].should.equal(10.5));
    it('should have correct first point latitude', () => deserialized.coordinates[0][1].should.equal(20.3));
    it('should have correct second point longitude', () => deserialized.coordinates[1][0].should.equal(30.1));
    it('should have correct second point latitude', () => deserialized.coordinates[1][1].should.equal(40.2));
    it('should have correct third point longitude', () => deserialized.coordinates[2][0].should.equal(50.7));
    it('should have correct third point latitude', () => deserialized.coordinates[2][1].should.equal(60.8));
});
