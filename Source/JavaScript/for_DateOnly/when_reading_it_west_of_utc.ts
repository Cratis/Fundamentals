// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';

/**
 * The reason this type exists. A calendar date forced into a `Date` becomes UTC midnight, and every browser-local
 * getter west of UTC then reads it back as the previous day - correct in Europe, wrong in the Americas, which is
 * how the mistake survives development and reaches users.
 */
describe('when reading it west of utc', () => {
    const dueDate = DateOnly.parse('2026-05-12');

    const asAnInstant = new Date('2026-05-12');
    const instantWestOfUtc = new Intl.DateTimeFormat('en-CA', { timeZone: 'America/New_York' }).format(asAnInstant);

    it('should be the day the server sent, whatever zone reads it', () => dueDate.toString().should.equal('2026-05-12'));

    // Pins the behavior being avoided, so the reason for the type cannot quietly stop being true.
    it('should not be what an instant would have rendered as', () => instantWestOfUtc.should.equal('2026-05-11'));

    it('should keep the day when a local Date is asked for', () => dueDate.toDate().getDate().should.equal(12));
    it('should keep the month when a local Date is asked for', () => (dueDate.toDate().getMonth() + 1).should.equal(5));
});
