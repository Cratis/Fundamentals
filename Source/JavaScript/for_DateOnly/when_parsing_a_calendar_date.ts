// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';

describe('when parsing a calendar date', () => {
    const parsed = DateOnly.parse('2026-05-12');

    it('should have the year', () => parsed.year.should.equal(2026));
    it('should have the month', () => parsed.month.should.equal(5));
    it('should have the day', () => parsed.day.should.equal(12));
    it('should render back to what it was parsed from', () => parsed.toString().should.equal('2026-05-12'));
});
