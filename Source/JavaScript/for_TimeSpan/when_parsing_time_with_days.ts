// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing time with days', () => {
    const timeSpanString = '5.12:30:45';
    const parsed = TimeSpan.parse(timeSpanString);

    it('should have correct days', () => parsed.days.should.equal(5));
    it('should have correct hours', () => parsed.hours.should.equal(12));
    it('should have correct minutes', () => parsed.minutes.should.equal(30));
    it('should have correct seconds', () => parsed.seconds.should.equal(45));
});
