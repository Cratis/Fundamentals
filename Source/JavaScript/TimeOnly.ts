// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from './fieldDecorator';
import { typeKey } from './typeKey';

const timeOnlyRegex = /^(\d{2}):(\d{2})(?::(\d{2}))?(?:\.(\d{1,7}))?$/;

/**
 * Represents a time of day with no date and no time zone.
 * @remarks
 * Deliberately not a JavaScript `Date`. A `Date` needs a date, and a time of day has none: `new Date('14:30:45')`
 * is not an instant pinned to some default day, it is `Invalid Date`. The value is destroyed outright rather than
 * merely shifted, which makes this the starker of the two temporal cases.
 */
export class TimeOnly {
    static readonly [typeKey] = 'TimeOnly';

    /**
     * The hour, 0 through 23.
     */
    @field(Number)
    hour!: number;

    /**
     * The minute, 0 through 59.
     */
    @field(Number)
    minute!: number;

    /**
     * The second, 0 through 59.
     */
    @field(Number)
    second!: number;

    /**
     * The millisecond, 0 through 999.
     */
    @field(Number)
    millisecond!: number;

    /**
     * Creates a {@link TimeOnly} from its parts.
     * @param hour The hour.
     * @param minute The minute.
     * @param second The second.
     * @param millisecond The millisecond.
     * @returns The {@link TimeOnly}.
     */
    static from(hour: number, minute: number, second: number = 0, millisecond: number = 0): TimeOnly {
        const timeOnly = new TimeOnly();
        timeOnly.hour = hour;
        timeOnly.minute = minute;
        timeOnly.second = second;
        timeOnly.millisecond = millisecond;
        return timeOnly;
    }

    /**
     * Parses the ISO-8601 time of day the server sends, `HH:mm`, `HH:mm:ss` or `HH:mm:ss.fffffff`.
     * @param value The value to parse.
     * @returns The {@link TimeOnly}.
     * @remarks
     * The seconds and the fraction are both optional because the server omits them when they are zero, and the
     * fraction carries up to seven digits where JavaScript holds three - so it is truncated rather than rounded,
     * which keeps a parse followed by a render from moving the value forward.
     */
    static parse(value: string): TimeOnly {
        const match = timeOnlyRegex.exec(value);
        if (match === null) {
            throw new Error(`Invalid TimeOnly format: ${value}`);
        }

        const fraction = match[4] ? match[4].padEnd(3, '0').substring(0, 3) : '0';

        return TimeOnly.from(
            parseInt(match[1], 10),
            parseInt(match[2], 10),
            match[3] ? parseInt(match[3], 10) : 0,
            parseInt(fraction, 10));
    }

    /**
     * Gets the ISO-8601 representation, `HH:mm:ss` - or `HH:mm:ss.fff` when there is a fractional part.
     * @returns The string.
     */
    toString(): string {
        const hour = this.hour.toString().padStart(2, '0');
        const minute = this.minute.toString().padStart(2, '0');
        const second = this.second.toString().padStart(2, '0');
        const time = `${hour}:${minute}:${second}`;
        return this.millisecond > 0 ? `${time}.${this.millisecond.toString().padStart(3, '0')}` : time;
    }

    /**
     * Determines whether this is the same time of day as another.
     * @param other The other time.
     * @returns True when they are the same time.
     */
    equals(other: TimeOnly | undefined | null): boolean {
        return !!other &&
            this.hour === other.hour &&
            this.minute === other.minute &&
            this.second === other.second &&
            this.millisecond === other.millisecond;
    }
}
