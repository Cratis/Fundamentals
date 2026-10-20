---
description: Parse, format and serialize time intervals in TypeScript using TimeSpan, compatible with .NET System.TimeSpan strings.
---
# TimeSpan

`TimeSpan` represents a duration rather than a calendar date or time of day. Use it to exchange intervals with a .NET backend using `System.TimeSpan` string formatting.

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

`TimeSpan.toJSON()` returns the same C#-compatible representation as `toString()`, so JSON payloads round-trip cleanly between frontend and backend.

```typescript
import { TimeSpan } from '@cratis/fundamentals';

const span = TimeSpan.parse('00:00:01.5');
const json = JSON.stringify({ duration: span });
```

## Available Values

A parsed `TimeSpan` provides component and aggregate values, including:

- `ticks`
- `days`, `hours`, `minutes`, `seconds`
- `milliseconds`, `microseconds`, `nanoseconds`
- `totalDays`, `totalHours`, `totalMinutes`, `totalSeconds`
- `totalMilliseconds`, `totalMicroseconds`, `totalNanoseconds`

Use aggregate values for duration comparisons: `totalHours` includes whole days, while `hours` is only the remaining hour component. For a negative duration, `ticks` and the aggregate values carry the sign; the hour, minute, and second components remain magnitudes. Tick values use JavaScript numbers, so do not assume exact integer precision for very large intervals.

## See also

- [TimeOnly](time_only.md) — represent a time of day
- [DateOnly](date_only.md) — represent a calendar date
- [JsonSerializer](serialization/json_serializer.md) — work with typed JSON payloads
