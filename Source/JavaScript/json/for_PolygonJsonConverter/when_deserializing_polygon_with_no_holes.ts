// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Polygon } from '../../geospatial/Polygon';
import { JsonSerializer } from '../../JsonSerializer';

describe('when deserializing polygon with no holes', () => {
    const json = '{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}';
    const result = JsonSerializer.deserialize(Polygon, json);

    it('should be a Polygon', () => result.should.be.instanceof(Polygon));
    it('should have shell with five points', () => result.shell.coordinates.length.should.equal(5));
    it('should have no holes', () => result.holes.length.should.equal(0));
    it('should have correct first point in shell', () => {
        result.shell.coordinates[0].longitude.should.equal(0);
        result.shell.coordinates[0].latitude.should.equal(0);
    });
    it('should have correct last point in shell (closed ring)', () => {
        result.shell.coordinates[4].longitude.should.equal(0);
        result.shell.coordinates[4].latitude.should.equal(0);
    });
});
