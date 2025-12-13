// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing negative time', () => {
    const timeSpanString = '-2.03:15:30';
    const parsed = TimeSpan.parse(timeSpanString);

    it('should have negative days', () => parsed.days.should.equal(-2));
    it('should have correct hours', () => parsed.hours.should.equal(3));
    it('should have correct minutes', () => parsed.minutes.should.equal(15));
    it('should have correct seconds', () => parsed.seconds.should.equal(30));
    it('should have negative ticks', () => parsed.ticks.should.be.lessThan(0));
});
