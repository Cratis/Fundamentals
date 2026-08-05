// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from './fieldDecorator';
import { typeKey } from './typeKey';

const dateOnlyRegex = /^(\d{4})-(\d{2})-(\d{2})$/;

/**
 * Represents a date with no time and no time zone - a day on a calendar.
 * @remarks
 * Deliberately not a JavaScript `Date`. A `Date` is an instant, and a calendar date is not one: turning
 * `2026-05-12` into a `Date` produces UTC midnight, which every browser-local getter west of UTC reads back as
 * the 11th. That is a wrong answer that looks right, and it is only wrong for users in some time zones - which is
 * how it survives development in others.
 *
 * Holding the three parts the server sent means there is no instant to convert and nothing to shift. Where a
 * `Date` is genuinely wanted - to feed a date picker, or to do arithmetic - {@link toDate} constructs one in the
 * local time zone, and the choice is then visible at the call site making it.
 */
export class DateOnly {
    static readonly [typeKey] = 'DateOnly';

    /**
     * The year.
     */
    @field(Number)
    year!: number;

    /**
     * The month, 1 through 12.
     */
    @field(Number)
    month!: number;

    /**
     * The day of the month, 1 through 31.
     */
    @field(Number)
    day!: number;

    /**
     * Creates a {@link DateOnly} from its parts.
     * @param year The year.
     * @param month The month, 1 through 12.
     * @param day The day of the month.
     * @returns The {@link DateOnly}.
     */
    static from(year: number, month: number, day: number): DateOnly {
        const dateOnly = new DateOnly();
        dateOnly.year = year;
        dateOnly.month = month;
        dateOnly.day = day;
        return dateOnly;
    }

    /**
     * Parses the ISO-8601 calendar date the server sends, `yyyy-MM-dd`.
     * @param value The value to parse.
     * @returns The {@link DateOnly}.
     */
    static parse(value: string): DateOnly {
        const match = dateOnlyRegex.exec(value);
        if (match === null) {
            throw new Error(`Invalid DateOnly format: ${value}`);
        }

        return DateOnly.from(parseInt(match[1], 10), parseInt(match[2], 10), parseInt(match[3], 10));
    }

    /**
     * Creates a {@link DateOnly} for the calendar date a {@link Date} falls on in the local time zone.
     * @param date The date to take the calendar date of.
     * @returns The {@link DateOnly}.
     * @remarks
     * Reads the local parts rather than the UTC ones, because the calendar date a moment falls on is a question
     * only a time zone can answer, and the local one is the one the person looking at the screen is in.
     */
    static fromDate(date: Date): DateOnly {
        return DateOnly.from(date.getFullYear(), date.getMonth() + 1, date.getDate());
    }

    /**
     * Converts to a {@link Date} at midnight in the local time zone.
     * @returns The {@link Date}.
     * @remarks
     * Local rather than UTC, so the date a caller reads back with `getDate()` is the one they started with. This
     * invents a time that was never sent, which is why it is a method to call rather than what the value is.
     */
    toDate(): Date {
        return new Date(this.year, this.month - 1, this.day);
    }

    /**
     * Gets the ISO-8601 representation, `yyyy-MM-dd` - the same form the server sent.
     * @returns The string.
     */
    toString(): string {
        const month = this.month.toString().padStart(2, '0');
        const day = this.day.toString().padStart(2, '0');
        return `${this.year.toString().padStart(4, '0')}-${month}-${day}`;
    }

    /**
     * Determines whether this is the same calendar date as another.
     * @param other The other date.
     * @returns True when they are the same date.
     */
    equals(other: DateOnly | undefined | null): boolean {
        return !!other && this.year === other.year && this.month === other.month && this.day === other.day;
    }
}
