// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Point } from '../geospatial/Point';
import { PointJsonConverter } from '../json/PointJsonConverter';

describe('when writing point with point converter', () => {
    const converter = new PointJsonConverter();
    const point = new Point(10.5, 20.3);
    const result = converter.write(point);

    it('should have type property', () => result.type.should.equal('Point'));
    it('should have coordinates array', () => result.coordinates.should.be.instanceof(Array));
    it('should have longitude as first coordinate', () => result.coordinates[0].should.equal(10.5));
    it('should have latitude as second coordinate', () => result.coordinates[1].should.equal(20.3));
});
