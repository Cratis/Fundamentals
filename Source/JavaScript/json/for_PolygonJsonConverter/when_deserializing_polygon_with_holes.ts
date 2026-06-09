// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Polygon } from '../../geospatial/Polygon';
import { JsonSerializer } from '../../JsonSerializer';

describe('when deserializing polygon with holes', () => {
    const json = '{"type":"Polygon","coordinates":[[[0,0],[20,0],[20,20],[0,20],[0,0]],[[2,2],[8,2],[8,8],[2,8],[2,2]],[[12,12],[18,12],[18,18],[12,18],[12,12]]]}';
    const result = JsonSerializer.deserialize(Polygon, json);

    it('should be a Polygon', () => result.should.be.instanceof(Polygon));
    it('should have shell with five points', () => result.shell.coordinates.length.should.equal(5));
    it('should have two holes', () => result.holes.length.should.equal(2));
    it('should have five points in first hole', () => result.holes[0].coordinates.length.should.equal(5));
    it('should have five points in second hole', () => result.holes[1].coordinates.length.should.equal(5));
    it('should have correct first point in shell', () => {
        result.shell.coordinates[0].longitude.should.equal(0);
        result.shell.coordinates[0].latitude.should.equal(0);
    });
    it('should have correct first point in first hole', () => {
        result.holes[0].coordinates[0].longitude.should.equal(2);
        result.holes[0].coordinates[0].latitude.should.equal(2);
    });
    it('should have correct first point in second hole', () => {
        result.holes[1].coordinates[0].longitude.should.equal(12);
        result.holes[1].coordinates[0].latitude.should.equal(12);
    });
});
