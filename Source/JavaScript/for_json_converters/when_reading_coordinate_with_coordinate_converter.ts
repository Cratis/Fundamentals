// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Coordinate } from '../Coordinate';
import { CoordinateJsonConverter } from '../json/CoordinateJsonConverter';

describe('when reading coordinate with coordinate converter', () => {
    const converter = new CoordinateJsonConverter();
    const result = converter.read({ longitude: 10.5, latitude: 20.3 });

    it('should return Coordinate instance', () => result.should.be.instanceof(Coordinate));
    it('should have correct longitude', () => result.longitude.should.equal(10.5));
    it('should have correct latitude', () => result.latitude.should.equal(20.3));
});
