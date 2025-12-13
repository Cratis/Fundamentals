// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { field } from './fieldDecorator';

const timeSpanRegex = /^(-)?(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})(?:\.(\d{1,7}))?$/;

/**
 * Represents a time interval.
 */
export class TimeSpan {
   
    @field(Number)
    ticks!: number;

    @field(Number)
    days!: number;

    @field(Number)
    hours!: number;

    @field(Number)
    milliseconds!: number;

    @field(Number)
    microseconds!: number;

    @field(Number)
    nanoseconds!: number;

    @field(Number)
    minutes!: number;

    @field(Number)
    seconds!: number;

    @field(Number)
    totalDays!: number;

    @field(Number)
    totalHours!: number;

    @field(Number)
    totalMilliseconds!: number;

    @field(Number)
    totalMicroseconds!: number;

    @field(Number)
    totalNanoseconds!: number;

    @field(Number)
    totalMinutes!: number;

    @field(Number)
    totalSeconds!: number;

    /**
     * Parses a C# compatible TimeSpan string and returns a TimeSpan instance.
     * The string format should match: [-][d.]hh:mm:ss[.fffffff]
     * @param {string} value String representation of TimeSpan.
     * @returns {TimeSpan} A TimeSpan instance with all properties populated.
     * @throws {Error} If the string format is invalid.
     */
    static parse(value: string): TimeSpan {
        const match = timeSpanRegex.exec(value);
        if (match === null) {
            throw new Error(`Invalid TimeSpan format: ${value}`);
        }

        const isNegative = match[1] === '-';
        const days = match[2] ? parseInt(match[2], 10) : 0;
        const hours = parseInt(match[3], 10);
        const minutes = parseInt(match[4], 10);
        const seconds = parseInt(match[5], 10);
        const fractionalSeconds = match[6] ? match[6].padEnd(7, '0') : '0000000';

        const ticksPerDay = 864000000000;
        const ticksPerHour = 36000000000;
        const ticksPerMinute = 600000000;
        const ticksPerSecond = 10000000;

        const ticks =
            days * ticksPerDay +
            hours * ticksPerHour +
            minutes * ticksPerMinute +
            seconds * ticksPerSecond +
            parseInt(fractionalSeconds, 10);

        const finalTicks = isNegative ? -ticks : ticks;

        const timeSpan = new TimeSpan();
        timeSpan.ticks = finalTicks;
        timeSpan.days = Math.floor(Math.abs(finalTicks) / ticksPerDay) * (isNegative ? -1 : 1);
        timeSpan.hours = Math.floor((Math.abs(finalTicks) / ticksPerHour) % 24);
        timeSpan.minutes = Math.floor((Math.abs(finalTicks) / ticksPerMinute) % 60);
        timeSpan.seconds = Math.floor((Math.abs(finalTicks) / ticksPerSecond) % 60);
        timeSpan.milliseconds = Math.floor((Math.abs(finalTicks) / 10000) % 1000);
        timeSpan.microseconds = Math.floor((Math.abs(finalTicks) / 10) % 1000);
        timeSpan.nanoseconds = Math.floor((Math.abs(finalTicks) % 10) * 100);
        timeSpan.totalDays = finalTicks / ticksPerDay;
        timeSpan.totalHours = finalTicks / ticksPerHour;
        timeSpan.totalMinutes = finalTicks / ticksPerMinute;
        timeSpan.totalSeconds = finalTicks / ticksPerSecond;
        timeSpan.totalMilliseconds = finalTicks / 10000;
        timeSpan.totalMicroseconds = finalTicks / 10;
        timeSpan.totalNanoseconds = finalTicks * 100;

        return timeSpan;
    }

    /**
     * Converts the TimeSpan to a C# compatible string format.
     * Returns the TimeSpan in format: [-][d.]hh:mm:ss[.fffffff]
     * @returns {string} C# compatible TimeSpan string representation.
     */
    toString(): string {
        const isNegative = this.ticks < 0;
        const absTicks = Math.abs(this.ticks);

        const ticksPerDay = 864000000000;
        const ticksPerHour = 36000000000;
        const ticksPerMinute = 600000000;
        const ticksPerSecond = 10000000;

        const days = Math.floor(absTicks / ticksPerDay);
        const hours = Math.floor((absTicks / ticksPerHour) % 24);
        const minutes = Math.floor((absTicks / ticksPerMinute) % 60);
        const seconds = Math.floor((absTicks / ticksPerSecond) % 60);
        const fractionalTicks = absTicks % ticksPerSecond;

        const parts: string[] = [];

        if (isNegative) {
            parts.push('-');
        }

        if (days > 0) {
            parts.push(`${days}.`);
        }

        parts.push(`${hours.toString().padStart(2, '0')}:`);
        parts.push(`${minutes.toString().padStart(2, '0')}:`);
        parts.push(`${seconds.toString().padStart(2, '0')}`);

        if (fractionalTicks > 0) {
            const fractionalStr = fractionalTicks.toString().padStart(7, '0').replace(/0+$/, '');
            parts.push(`.${fractionalStr}`);
        }

        return parts.join('');
    }

    /**
     * Converts the TimeSpan to a JSON string that works in JSON serialization.
     * Returns the TimeSpan in C# compatible format: [-][d.]hh:mm:ss[.fffffff]
     * @returns {string} C# compatible TimeSpan string representation.
     */
    toJSON(): string {
        return this.toString();
    }
}
