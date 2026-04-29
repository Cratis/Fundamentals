# TimeSpan

`TimeSpan` represents a time interval compatible with .NET `System.TimeSpan` string formatting.

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

## Formatting

Use `toString()` to produce a C#-compatible `TimeSpan` string.

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
