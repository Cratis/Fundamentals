// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing zero time', () => {
    const timeSpanString = '00:00:00';
    const parsed = TimeSpan.parse(timeSpanString);

    it('should have zero ticks', () => parsed.ticks.should.equal(0));
    it('should have zero days', () => parsed.days.should.equal(0));
    it('should have zero hours', () => parsed.hours.should.equal(0));
    it('should have zero minutes', () => parsed.minutes.should.equal(0));
    it('should have zero seconds', () => parsed.seconds.should.equal(0));
    it('should have zero total milliseconds', () => parsed.totalMilliseconds.should.equal(0));
});
