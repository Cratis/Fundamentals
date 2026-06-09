// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { LineString } from '../geospatial/LineString';
import { JsonSerializer } from '../JsonSerializer';

describe('when deserializing linestring', () => {
    const json = '{"type":"LineString","coordinates":[[10.5,20.3],[30.1,40.2],[50.7,60.8]]}';
    const result = JsonSerializer.deserialize(LineString, json);

    it('should be a LineString', () => result.should.be.instanceof(LineString));
    it('should have three points', () => result.coordinates.length.should.equal(3));
    it('should have correct first point longitude', () => result.coordinates[0].longitude.should.equal(10.5));
    it('should have correct first point latitude', () => result.coordinates[0].latitude.should.equal(20.3));
    it('should have correct second point longitude', () => result.coordinates[1].longitude.should.equal(30.1));
    it('should have correct second point latitude', () => result.coordinates[1].latitude.should.equal(40.2));
    it('should have correct third point longitude', () => result.coordinates[2].longitude.should.equal(50.7));
    it('should have correct third point latitude', () => result.coordinates[2].latitude.should.equal(60.8));
});
