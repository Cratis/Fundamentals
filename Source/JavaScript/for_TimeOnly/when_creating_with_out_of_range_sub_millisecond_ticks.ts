// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

describe('when creating with out of range sub millisecond ticks', () => {
    it('should reject ticks above 9999', () => (() => TimeOnly.from(1, 2, 3, 4, 10000)).should.throw('subMillisecondTicks 10000 is out of range'));
    it('should reject negative ticks', () => (() => TimeOnly.from(1, 2, 3, 4, -1)).should.throw('subMillisecondTicks -1 is out of range'));
    it('should reject fractional ticks', () => (() => TimeOnly.from(1, 2, 3, 4, 1.5)).should.throw('out of range'));
    it('should accept the largest value', () => TimeOnly.from(1, 2, 3, 4, 9999).subMillisecondTicks.should.equal(9999));
});
