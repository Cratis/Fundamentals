// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { JsonSerializer } from '../JsonSerializer';
import { DateJsonConverter, JsonConverter } from '../json';
import { field } from '../fieldDecorator';

/* eslint-disable @typescript-eslint/no-explicit-any */

/**
 * Writes a Date as a calendar day rather than an instant - the shape a consumer reaches for when a
 * value that is a date on the wire keeps arriving as UTC midnight and rendering a day early.
 */
class CalendarDateJsonConverter extends JsonConverter<Date> {
    get type(): Constructor<Date> {
        return Date;
    }

    read(value: any): Date {
        return new Date(`${value}T00:00:00`);
    }

    write(value: Date): any {
        return value.toISOString().substring(0, 10);
    }
}

class Booking {
    @field(Date)
    arrivesOn!: Date;
}

describe('when a converter is registered for a type the package already converts', () => {
    const booking = new Booking();
    booking.arrivesOn = new Date(Date.UTC(2026, 7, 4, 9, 30));

    const beforeRegistering = JSON.parse(JsonSerializer.serialize(booking));

    JsonSerializer.registerConverter(new CalendarDateJsonConverter());
    const afterRegistering = JSON.parse(JsonSerializer.serialize(booking));

    // Put the built-in back before any assertion runs, so a failing expectation cannot leave the
    // registry - which is module scope shared by every spec file - holding this spec's converter.
    JsonSerializer.registerConverter(new DateJsonConverter());
    const afterRestoring = JSON.parse(JsonSerializer.serialize(booking));

    it('should use the built-in converter until one is registered', () => beforeRegistering.arrivesOn.should.equal('2026-08-04T09:30:00.000Z'));
    it('should take the place of the built-in converter', () => afterRegistering.arrivesOn.should.equal('2026-08-04'));
    it('should hand back to a built-in converter registered after it', () => afterRestoring.arrivesOn.should.equal('2026-08-04T09:30:00.000Z'));
});
