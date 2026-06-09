// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateJsonConverter } from '../DateJsonConverter';

describe('when reading date with date converter', () => {
    const converter = new DateJsonConverter();
    const result = converter.read('2024-01-15T10:30:00.000Z');

    it('should return Date instance', () => result.should.be.instanceof(Date));
    it('should have correct time', () => result.getTime().should.equal(new Date('2024-01-15T10:30:00.000Z').getTime()));
});
