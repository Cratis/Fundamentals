// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Coordinate } from '../Coordinate';
import { CoordinateJsonConverter } from '../json/CoordinateJsonConverter';

describe('when writing coordinate with coordinate converter', () => {
    const converter = new CoordinateJsonConverter();
    const coordinate = new Coordinate(10.5, 20.3);
    const result = converter.write(coordinate);

    it('should have longitude property', () => result.longitude.should.equal(10.5));
    it('should have latitude property', () => result.latitude.should.equal(20.3));
});
