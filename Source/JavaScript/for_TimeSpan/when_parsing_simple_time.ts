// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing simple time', () => {
    const timeSpanString = '01:30:45';
    const parsed = TimeSpan.parse(timeSpanString);

    it('should have correct hours', () => parsed.hours.should.equal(1));
    it('should have correct minutes', () => parsed.minutes.should.equal(30));
    it('should have correct seconds', () => parsed.seconds.should.equal(45));
    it('should have zero days', () => parsed.days.should.equal(0));
    it('should have zero milliseconds', () => parsed.milliseconds.should.equal(0));
});
