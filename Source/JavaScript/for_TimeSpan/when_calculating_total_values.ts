// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when calculating total values', () => {
    const timeSpanString = '1.02:30:45';
    const parsed = TimeSpan.parse(timeSpanString);

    it('should have correct total hours', () => parsed.totalHours.should.equal(26.5125));
    it('should have correct total minutes', () => parsed.totalMinutes.should.equal(1590.75));
    it('should have correct total seconds', () => parsed.totalSeconds.should.equal(95445));
    it('should have correct total milliseconds', () => parsed.totalMilliseconds.should.equal(95445000));
});
