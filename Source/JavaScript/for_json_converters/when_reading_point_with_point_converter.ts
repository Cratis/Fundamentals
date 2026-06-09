// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../geospatial/Point';
import { PointJsonConverter } from '../json/PointJsonConverter';

describe('when reading point with point converter', () => {
    const converter = new PointJsonConverter();
    const result = converter.read({ type: 'Point', coordinates: [10.5, 20.3] });

    it('should return Point instance', () => result.should.be.instanceof(Point));
    it('should have correct longitude', () => result.longitude.should.equal(10.5));
    it('should have correct latitude', () => result.latitude.should.equal(20.3));
});
