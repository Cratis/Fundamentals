// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Coordinate } from '../Coordinate';
import { JsonSerializer } from '../JsonSerializer';

describe('when deserializing coordinate', () => {
    const json = '{"longitude":10.5,"latitude":20.3}';
    const result = JsonSerializer.deserialize(Coordinate, json);

    it('should be a coordinate', () => result.should.be.instanceof(Coordinate));
    it('should have correct longitude', () => result.longitude.should.equal(10.5));
    it('should have correct latitude', () => result.latitude.should.equal(20.3));
});
