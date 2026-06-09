// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../../geospatial/Point';
import { JsonSerializer } from '../../JsonSerializer';

describe('when deserializing point', () => {
    const json = '{"type":"Point","coordinates":[10.5,20.3]}';
    const result = JsonSerializer.deserialize(Point, json);

    it('should be a Point', () => result.should.be.instanceof(Point));
    it('should have correct longitude', () => result.longitude.should.equal(10.5));
    it('should have correct latitude', () => result.latitude.should.equal(20.3));
});
