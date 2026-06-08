// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Coordinate } from '../Coordinate';

describe('when creating coordinate', () => {
    const coordinate = new Coordinate(10.5, 20.3);

    it('should have correct longitude', () => coordinate.longitude.should.equal(10.5));
    it('should have correct latitude', () => coordinate.latitude.should.equal(20.3));
});
