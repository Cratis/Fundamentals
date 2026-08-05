# DateOnly

`DateOnly` represents a date with no time and no time zone — a day on a calendar. It is the TypeScript counterpart of .NET's `System.DateOnly`.

## Why not a Date

A JavaScript `Date` is an instant, and a calendar date is not one. Turning `2026-05-12` into a `Date` produces UTC midnight, which every browser-local getter west of UTC reads back as the 11th:

```typescript
// A birthday, a due date, an invoice date — none of them are instants
const wrong = new Date('2026-05-12');
console.log(wrong.getDate()); // 11 in New York, 12 in Oslo
```

That is a wrong answer that looks right, and it is only wrong for users in some time zones — which is how it survives development in others. `DateOnly` holds the three parts the server sent, so there is no instant to convert and nothing to shift.

## Creating

Use `parse()` for the ISO-8601 form the server sends, `from()` for known parts, and `fromDate()` to take the calendar date a `Date` falls on.

```typescript
import { DateOnly } from '@cratis/fundamentals';

const parsed = DateOnly.parse('2026-05-12');
const constructed = DateOnly.from(2026, 5, 12);
const today = DateOnly.fromDate(new Date());
```

`parse()` accepts `yyyy-MM-dd` and throws an `Error` for anything else. `fromDate()` reads the local parts rather than the UTC ones, because the calendar date a moment falls on is a question only a time zone can answer, and the local one is the one the person looking at the screen is in.

## Available values

```typescript
import { DateOnly } from '@cratis/fundamentals';

const date = DateOnly.parse('2026-05-12');

console.log(date.year);  // 2026
console.log(date.month); // 5  — 1 through 12, not 0-based like Date
console.log(date.day);   // 12
```

Note that `month` is 1 through 12, matching the wire format and .NET, rather than the 0-based month a `Date` uses.

## Formatting

`toString()` produces the same ISO-8601 form the server sent.

```typescript
import { DateOnly } from '@cratis/fundamentals';

const date = DateOnly.from(2026, 5, 12);
console.log(date.toString()); // "2026-05-12"
```

## Converting to a Date

Where a `Date` is genuinely wanted — to feed a date picker, or to do arithmetic — `toDate()` constructs one at midnight in the local time zone, so the date read back with `getDate()` is the one you started with.

```typescript
import { DateOnly } from '@cratis/fundamentals';

const date = DateOnly.parse('2026-05-12');
const asDate = date.toDate();
console.log(asDate.getDate()); // 12, in every time zone
```

This invents a time that was never sent, which is why it is a method to call rather than what the value is — the choice is visible at the call site making it.

## Equality

```typescript
import { DateOnly } from '@cratis/fundamentals';

const first = DateOnly.parse('2026-05-12');
const second = DateOnly.from(2026, 5, 12);

console.log(first.equals(second));                     // true
console.log(first.equals(DateOnly.from(2026, 5, 13))); // false
console.log(first.equals(undefined));                  // false
```

## JSON serialization

`DateOnlyJsonConverter` is registered with `JsonSerializer` out of the box, so a field declared as `DateOnly` deserializes from the ISO-8601 string the server sends and serializes back to it.

```typescript
import { DateOnly, field, JsonSerializer } from '@cratis/fundamentals';

export class Invoice {
    @field(String)
    number!: string;

    @field(DateOnly)
    issued!: DateOnly;
}

const invoice = JsonSerializer.deserialize(Invoice, '{"number":"INV-1","issued":"2026-05-12"}');
console.log(invoice.issued.day); // 12

const json = JsonSerializer.serialize(invoice); // {"number":"INV-1","issued":"2026-05-12"}
```

Serialization goes through `JsonSerializer`, not through `JSON.stringify` — `DateOnly` has no `toJSON()`, so `JSON.stringify` writes the individual parts instead of the ISO-8601 string.

## Related

- [TimeOnly](./time_only.md) — a time of day with no date and no time zone
- [JsonSerializer](./serialization/json_serializer.md) — how types are converted on the way in and out
- [@field Decorator](./serialization/field_decorator.md) — declaring the runtime type of a field
