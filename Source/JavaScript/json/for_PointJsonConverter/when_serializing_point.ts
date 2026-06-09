// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../../geospatial/Point';
import { JsonSerializer } from '../../JsonSerializer';

describe('when serializing point', () => {
    const point = new Point(10.5, 20.3);
    const result = JsonSerializer.serialize(point);
    const deserialized = JSON.parse(result);

    it('should have type of Point', () => deserialized.type.should.equal('Point'));
    it('should have coordinates array', () => deserialized.coordinates.should.be.an('array'));
    it('should have two coordinates', () => deserialized.coordinates.length.should.equal(2));
    it('should have correct longitude', () => deserialized.coordinates[0].should.equal(10.5));
    it('should have correct latitude', () => deserialized.coordinates[1].should.equal(20.3));
});
