# TimeOnly

`TimeOnly` represents a time of day with no date and no time zone. It is the TypeScript counterpart of .NET's `System.TimeOnly`.

## Why not a Date

A JavaScript `Date` needs a date, and a time of day has none:

```typescript
// An opening hour, an alarm, a shift start — none of them fall on a particular day
const wrong = new Date('14:30:45');
console.log(wrong.toString()); // "Invalid Date"
```

The value is destroyed outright rather than merely shifted, which makes this the starker of the two temporal cases. `TimeOnly` holds the parts the server sent, so there is nothing to convert.

## Creating

Use `parse()` for the ISO-8601 form the server sends, and `from()` for known parts.

```typescript
import { TimeOnly } from '@cratis/fundamentals';

const parsed = TimeOnly.parse('14:30:45');
const constructed = TimeOnly.from(14, 30, 45);
const opening = TimeOnly.from(9, 0); // seconds and milliseconds default to 0
```

`parse()` accepts `HH:mm`, `HH:mm:ss` and `HH:mm:ss.fffffff`, and throws an `Error` for anything else. The seconds and the fraction are both optional because the server omits them when they are zero.

## Available values

```typescript
import { TimeOnly } from '@cratis/fundamentals';

const time = TimeOnly.parse('14:30:45.250');

console.log(time.hour);        // 14
console.log(time.minute);      // 30
console.log(time.second);      // 45
console.log(time.millisecond); // 250
```

.NET carries up to seven fractional digits where JavaScript holds three, so the fraction is truncated rather than rounded — which keeps a parse followed by a render from moving the value forward.

## Formatting

`toString()` produces `HH:mm:ss`, or `HH:mm:ss.fff` when there is a fractional part.

```typescript
import { TimeOnly } from '@cratis/fundamentals';

console.log(TimeOnly.from(14, 30, 45).toString());      // "14:30:45"
console.log(TimeOnly.from(14, 30, 45, 250).toString()); // "14:30:45.250"
console.log(TimeOnly.from(9, 0).toString());            // "09:00:00"
```

## Equality

```typescript
import { TimeOnly } from '@cratis/fundamentals';

const first = TimeOnly.parse('14:30:45');
const second = TimeOnly.from(14, 30, 45);

console.log(first.equals(second));                    // true
console.log(first.equals(TimeOnly.from(14, 30, 46))); // false
console.log(first.equals(undefined));                 // false
```

## JSON serialization

`TimeOnlyJsonConverter` is registered with `JsonSerializer` out of the box, so a field declared as `TimeOnly` deserializes from the ISO-8601 string the server sends and serializes back to it.

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

Serialization goes through `JsonSerializer`, not through `JSON.stringify` — `TimeOnly` has no `toJSON()`, so `JSON.stringify` writes the individual parts instead of the ISO-8601 string.

## Related

- [DateOnly](./date_only.md) — a date with no time and no time zone
- [TimeSpan](./time_span.md) — an interval of time rather than a point in the day
- [JsonSerializer](./serialization/json_serializer.md) — how types are converted on the way in and out
