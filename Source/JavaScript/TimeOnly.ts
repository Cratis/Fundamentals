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
     * The remaining 100 nanosecond ticks below the millisecond, 0 through 9999. Carries the last four digits of
     * the seven-digit fraction a C# `TimeOnly` holds, so two times that differ below a millisecond stay different.
     */
    @field(Number)
    subMillisecondTicks!: number;

    /**
     * Creates a {@link TimeOnly} from its parts.
     * @param hour The hour.
     * @param minute The minute.
     * @param second The second.
     * @param millisecond The millisecond.
     * @param subMillisecondTicks The 100 nanosecond ticks below the millisecond, 0 through 9999.
     * @returns The {@link TimeOnly}.
     * @throws {Error} If `subMillisecondTicks` is not a whole number from 0 through 9999.
     */
    static from(hour: number, minute: number, second: number = 0, millisecond: number = 0, subMillisecondTicks: number = 0): TimeOnly {
        if (!Number.isInteger(subMillisecondTicks) || subMillisecondTicks < 0 || subMillisecondTicks > 9999) {
            throw new Error(`Invalid TimeOnly: subMillisecondTicks ${subMillisecondTicks} is out of range 0-9999`);
        }

        const timeOnly = new TimeOnly();
        timeOnly.hour = hour;
        timeOnly.minute = minute;
        timeOnly.second = second;
        timeOnly.millisecond = millisecond;
        timeOnly.subMillisecondTicks = subMillisecondTicks;
        return timeOnly;
    }

    /**
     * Parses the ISO-8601 time of day the server sends, `HH:mm`, `HH:mm:ss` or `HH:mm:ss.fffffff`.
     * @param value The value to parse.
     * @returns The {@link TimeOnly}.
     * @remarks
     * The seconds and the fraction are both optional because the server omits them when they are zero. The
     * fraction carries up to seven digits, all of which are kept, so C# times that differ below a millisecond
     * stay distinct. Components outside the ranges C# accepts are rejected.
     * @throws {Error} If the format is invalid or a component is out of range.
     */
    static parse(value: string): TimeOnly {
        const match = timeOnlyRegex.exec(value);
        if (match === null) {
            throw new Error(`Invalid TimeOnly format: ${value}`);
        }

        const hour = parseInt(match[1], 10);
        const minute = parseInt(match[2], 10);
        const second = match[3] ? parseInt(match[3], 10) : 0;

        if (hour > 23) {
            throw new Error(`Invalid TimeOnly: hour ${hour} is out of range 0-23 in '${value}'`);
        }

        if (minute > 59) {
            throw new Error(`Invalid TimeOnly: minute ${minute} is out of range 0-59 in '${value}'`);
        }

        if (second > 59) {
            throw new Error(`Invalid TimeOnly: second ${second} is out of range 0-59 in '${value}'`);
        }

        const fraction = (match[4] ?? '').padEnd(7, '0');

        return TimeOnly.from(
            hour,
            minute,
            second,
            parseInt(fraction.substring(0, 3), 10),
            parseInt(fraction.substring(3), 10));
    }

    /**
     * Gets the ISO-8601 representation: `HH:mm:ss`, or `HH:mm:ss.fff` when there is a millisecond part, or the
     * full seven-digit `HH:mm:ss.fffffff` when there is a non-zero part below the millisecond. The seven-digit form
     * is padded, never trimmed, so the two fractional forms are the only ones produced; the parser accepts 1-7 digits.
     * @returns The string.
     */
    toString(): string {
        const hour = this.hour.toString().padStart(2, '0');
        const minute = this.minute.toString().padStart(2, '0');
        const second = this.second.toString().padStart(2, '0');
        const time = `${hour}:${minute}:${second}`;
        const subMillisecondTicks = this.subMillisecondTicks ?? 0;
        if (subMillisecondTicks > 0) {
            return `${time}.${this.millisecond.toString().padStart(3, '0')}${subMillisecondTicks.toString().padStart(4, '0')}`;
        }

        return this.millisecond > 0 ? `${time}.${this.millisecond.toString().padStart(3, '0')}` : time;
    }

    /**
     * Gets the ISO-8601 representation used when this is serialized as JSON.
     *
     * Without this, `JSON.stringify` falls back to enumerating `hour`, `minute`, `second` and
     * `millisecond`, so a request body carries a component object where the server expects the
     * scalar string.
     * @returns The string.
     */
    toJSON(): string {
        return this.toString();
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
            this.millisecond === other.millisecond &&
            (this.subMillisecondTicks ?? 0) === (other.subMillisecondTicks ?? 0);
    }
}
