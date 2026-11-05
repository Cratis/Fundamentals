---
title: TimeOnly
description: TimeOnly in Cratis Fundamentals represents a time of day without a date or time zone in TypeScript, with parsing and JSON serialization.
---

An opening time such as 09:00 does not identify an instant until you also choose a date and time zone. `TimeOnly` keeps just the time of day. Use `parse()` for an incoming time string, compare it with `equals()`, and send it back as a string without inventing a date.

## Why not a Date

A JavaScript `Date` needs a date, and a time of day has none:

```typescript
// An opening hour, an alarm, a shift start — none of them fall on a particular day
const wrong = new Date('14:30:45');
console.log(wrong.toString()); // "Invalid Date"
```

That string cannot construct a valid `Date`. `TimeOnly.parse()` accepts a time without a date, so you can work with the opening hour directly instead.

## Creating

Use `parse()` for the ISO-8601 form the server sends, and `from()` for known parts.

```typescript
import { TimeOnly } from '@cratis/fundamentals';

const parsed = TimeOnly.parse('14:30:45');
const constructed = TimeOnly.from(14, 30, 45);
const opening = TimeOnly.from(9, 0); // seconds and milliseconds default to 0
```

`parse()` accepts `HH:mm`, `HH:mm:ss`, or seconds followed by one to seven fractional digits. It checks hours 0–23 and minutes/seconds 0–59; invalid formats or out-of-range parts throw an `Error`. Accepted optional parts do not tell you which format a particular server will emit.

Use `from()` for known parts. Seconds, milliseconds and the fifth argument, `subMillisecondTicks`, default to zero. Only that fifth argument is validated by `from()`: it must be an integer from 0 to 9999. The hour, minute, second and millisecond arguments are assigned without range checks.

## Available values

```typescript
import { TimeOnly } from '@cratis/fundamentals';

const time = TimeOnly.parse('14:30:45.250');

console.log(time.hour);        // 14
console.log(time.minute);      // 30
console.log(time.second);      // 45
console.log(time.millisecond); // 250
console.log(time.subMillisecondTicks); // 0
```

Fractions are preserved to seven digits: `millisecond` holds the first three and `subMillisecondTicks` holds the remaining four, in 100-nanosecond units. The `.250` value above has no remainder below a millisecond; a longer fraction is not truncated to those three digits.

The component fields, including `subMillisecondTicks`, are writable and have no validating setters. Direct assignment bypasses the parser and factory checks above. If an opening time changes, parse a new valid time string rather than editing its parts.

## Formatting

`toString()` always includes seconds. With no fraction it produces `HH:mm:ss`; with milliseconds but no smaller remainder it adds three digits. A nonzero `subMillisecondTicks` remainder makes it write all seven fractional digits:

```typescript
import { TimeOnly } from '@cratis/fundamentals';

console.log(TimeOnly.from(14, 30, 45).toString());      // "14:30:45"
console.log(TimeOnly.from(14, 30, 45, 250).toString()); // "14:30:45.250"
console.log(TimeOnly.from(9, 0).toString());            // "09:00:00"
console.log(TimeOnly.parse('14:30:45.2500001').toString()); // "14:30:45.2500001"
```

## Equality

`equals()` compares every component, including the remainder below a millisecond. Two times can therefore differ even when their `millisecond` values match:

```typescript
import { TimeOnly } from '@cratis/fundamentals';

const first = TimeOnly.parse('14:30:45');
const second = TimeOnly.from(14, 30, 45);

console.log(first.equals(second));                    // true
console.log(first.equals(TimeOnly.from(14, 30, 46))); // false
console.log(first.equals(undefined));                 // false
console.log(TimeOnly.parse('14:30:45.2500001').equals(TimeOnly.parse('14:30:45.2509999'))); // false
```

## JSON serialization

When a response contains a model of opening hours, `@field(TimeOnly)` tells `JsonSerializer.deserialize()` to restore its time fields as instances. The built-in `TimeOnlyJsonConverter` handles their strings, so `hours.opens.hour` below reads the opening hour:

```typescript
import { field, JsonSerializer, TimeOnly } from '@cratis/fundamentals';

export class OpeningHours {
    @field(String)
    day!: string;

    @field(TimeOnly)
    opens!: TimeOnly;

    @field(TimeOnly)
    closes!: TimeOnly;
}

const hours = JsonSerializer.deserialize(
    OpeningHours,
    '{"day":"Monday","opens":"09:00:00","closes":"17:30:00"}');

console.log(hours.opens.hour); // 9

const json = JsonSerializer.serialize(hours);
```

To send an opening time in a request body, pass the `TimeOnly` value to `JSON.stringify`. Its `toJSON()` method writes the time as a string, so `JSON.stringify({ opens: TimeOnly.from(9, 0) })` produces `{"opens":"09:00:00"}`. Reading that JSON leaves `opens` as a string; use `TimeOnly.parse()` for the scalar, or [JsonSerializer](./serialization/json_serializer.md) with `@field(TimeOnly)` when you need an instance in a model.

## Related

- [DateOnly](./date_only.md) — a date with no time and no time zone
- [TimeSpan](./time_span.md) — an interval of time rather than a point in the day
- [JsonSerializer](./serialization/json_serializer.md) — how types are converted on the way in and out
