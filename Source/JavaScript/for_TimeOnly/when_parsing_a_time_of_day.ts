// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

describe('when parsing a time of day', () => {
    const parsed = TimeOnly.parse('14:30:45');

    it('should have the hour', () => parsed.hour.should.equal(14));
    it('should have the minute', () => parsed.minute.should.equal(30));
    it('should have the second', () => parsed.second.should.equal(45));
    it('should render back to what it was parsed from', () => parsed.toString().should.equal('14:30:45'));

    // A Date needs a date, and a time of day has none - so this is destroyed outright rather than shifted, which
    // is the starker of the two temporal cases and the reason the type is not a Date.
    it('should not be something a Date could have held', () => isNaN(new Date('14:30:45').getTime()).should.be.true);
});
