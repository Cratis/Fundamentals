// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing time beyond what a time span can hold', () => {
    it('should reject more days than the maximum', () => (() => TimeSpan.parse('10675200.00:00:00')).should.throw('days 10675200 is out of range'));
    it('should reject the maximum days with a remainder above the maximum', () => (() => TimeSpan.parse('10675199.02:48:05.4775808')).should.throw('outside the range'));
    it('should accept the maximum value', () => TimeSpan.parse('10675199.02:48:05.4775807').days.should.equal(10675199));
    it('should accept the minimum value', () => TimeSpan.parse('-10675199.02:48:05.4775808').days.should.equal(-10675199));
});
