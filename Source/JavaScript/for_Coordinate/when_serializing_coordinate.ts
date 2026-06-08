// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Coordinate } from '../Coordinate';
import { JsonSerializer } from '../JsonSerializer';

describe('when serializing coordinate', () => {
    const coordinate = new Coordinate(10.5, 20.3);
    const result = JsonSerializer.serialize(coordinate);
    const deserialized = JSON.parse(result);

    it('should have correct longitude', () => deserialized.longitude.should.equal(10.5));
    it('should have correct latitude', () => deserialized.latitude.should.equal(20.3));
});
