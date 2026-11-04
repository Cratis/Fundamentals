---
description: Parse, format and serialize time intervals in TypeScript using TimeSpan, compatible with .NET System.TimeSpan strings.
---
# TimeSpan

A timeout or elapsed interval is a duration, not a calendar date or an opening time. `TimeSpan.parse()` turns its string into components and aggregate values you can compare. Use `totalHours` when the whole interval matters, then `toString()` or JSON serialization when you need the duration string again.

## Parsing

Use `TimeSpan.parse()` to parse C#-compatible values in the format `[-][d.]hh:mm:ss[.fffffff]`.

```typescript
import { TimeSpan } from '@cratis/fundamentals';

const span = TimeSpan.parse('1.02:30:15.25');

console.log(span.days); // 1
console.log(span.hours); // 2
console.log(span.minutes); // 30
console.log(span.seconds); // 15
```

Parsing requires hours, minutes, and seconds; days and the fractional part are optional. Hours must be between 0 and 23, and minutes and seconds between 0 and 59. Express a full day as `1.00:00:00`, not `24:00:00`. Invalid formats and out-of-range values throw an `Error`. A leading minus sign represents a negative interval, and fractions accept one to seven digits.

## Formatting

Use `toString()` to produce a C#-compatible `TimeSpan` string. It pads the clock components, omits zero days, and removes trailing zeros from the fractional part. Create initialized values with `parse()` before formatting them.

```typescript
import { TimeSpan } from '@cratis/fundamentals';

const span = TimeSpan.parse('00:05:30');
console.log(span.toString()); // "00:05:30"
```

## JSON Serialization

`TimeSpan.toJSON()` returns the same duration string as `toString()`. Native `JSON.stringify()` calls it automatically, so this request body contains a string rather than the component fields:

```typescript
import { TimeSpan } from '@cratis/fundamentals';

const span = TimeSpan.parse('00:00:01.5');
const json = JSON.stringify({ duration: span });
console.log(json); // {"duration":"00:00:01.5"}
```

Plain `JSON.parse(json)` restores the JSON object but leaves `duration` as a string. Use `TimeSpan.parse(JSON.parse(json).duration)` when you need a duration instance again. For fields in a typed response model, see [JsonSerializer](serialization/json_serializer.md) rather than assuming JSON parsing restores classes.

## Available Values

A parsed `TimeSpan` provides component and aggregate values, including:

- `ticks`
- `days`, `hours`, `minutes`, `seconds`
- `milliseconds`, `microseconds`, `nanoseconds`
- `totalDays`, `totalHours`, `totalMinutes`, `totalSeconds`
- `totalMilliseconds`, `totalMicroseconds`, `totalNanoseconds`

Use aggregate values for duration comparisons: `totalHours` includes whole days, while `hours` is only the remaining hour component. In the parsing example, `span.hours` is 2 but `span.totalHours` is greater than 24. For a negative duration, `ticks` and the aggregate values carry the sign; the hour, minute, and second components remain magnitudes.

These public fields are writable independently. Assigning a new `hours` value does not recalculate `ticks` or `totalHours`, and `toString()` formats from `ticks`. To represent a changed duration consistently, parse the new duration string rather than treating component assignment as arithmetic.

Ticks and totals use JavaScript numbers, so very large intervals need not retain exact integer tick precision. Accepting seven fractional digits in the input does not remove that number-precision limit.

## See also

- [TimeOnly](time_only.md) — represent a time of day
- [DateOnly](date_only.md) — represent a calendar date
- [JsonSerializer](serialization/json_serializer.md) — work with typed JSON payloads
