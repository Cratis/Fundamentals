// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Polygon } from '../geospatial/Polygon';
import { PolygonJsonConverter } from '../json/PolygonJsonConverter';

describe('when reading polygon with polygon converter', () => {
    const converter = new PolygonJsonConverter();
    const result = converter.read({
        type: 'Polygon',
        coordinates: [
            [[0, 0], [10, 0], [10, 10], [0, 10], [0, 0]]
        ]
    });

    it('should return Polygon instance', () => result.should.be.instanceof(Polygon));
    it('should have shell ring with five points', () => result.shell.coordinates.length.should.equal(5));
    it('should have first point with correct coordinates', () => {
        result.shell.coordinates[0].longitude.should.equal(0);
        result.shell.coordinates[0].latitude.should.equal(0);
    });
    it('should have closed ring', () => {
        result.shell.coordinates[0].longitude.should.equal(result.shell.coordinates[4].longitude);
        result.shell.coordinates[0].latitude.should.equal(result.shell.coordinates[4].latitude);
    });
    it('should have no holes', () => result.holes.length.should.equal(0));
});
