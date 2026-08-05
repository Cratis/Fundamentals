// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';
import { field } from '../fieldDecorator';
import { JsonSerializer } from '../JsonSerializer';
import { TimeOnly } from '../TimeOnly';

class Appointment {
    @field(DateOnly)
    on!: DateOnly;

    @field(TimeOnly)
    at!: TimeOnly;
}

/**
 * The end of the chain the types exist for: what the server sends has to arrive as the value it names, through the
 * registered converters, without anyone at the call site parsing anything.
 */
describe('when deserializing temporal values', () => {
    const deserialized = JsonSerializer.deserializeFromInstance(Appointment, { on: '2026-05-12', at: '14:30:45' });

    it('should deserialize the date as a DateOnly', () => (deserialized.on instanceof DateOnly).should.be.true);
    it('should deserialize the time as a TimeOnly', () => (deserialized.at instanceof TimeOnly).should.be.true);
    it('should keep the calendar date', () => deserialized.on.toString().should.equal('2026-05-12'));
    it('should keep the time of day', () => deserialized.at.toString().should.equal('14:30:45'));

    it('should serialize back to what the server sent', () =>
        JSON.parse(JsonSerializer.serialize(deserialized)).should.deep.equal({ on: '2026-05-12', at: '14:30:45' }));
});
