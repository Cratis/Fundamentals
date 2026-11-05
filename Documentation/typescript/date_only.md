---
title: DateOnly
description: Represent a calendar date without time or time zone in TypeScript with DateOnly, the counterpart of .NET System.DateOnly.
---

An invoice date needs to stay the same calendar day when you display it or send it back to an API. `DateOnly` holds that date without a time or time zone. Parse the date string, use its calendar parts, and convert to a JavaScript `Date` only when another API actually needs an instant.

## Why not a Date

A JavaScript `Date` is an instant, and a calendar date is not one. Turning `2026-05-12` into a `Date` produces UTC midnight, so reading its local day west of UTC gives the 11th:

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

`parse()` requires `yyyy-MM-dd` and checks that the year, month and day form a valid calendar date; an impossible date such as `2026-02-30` throws an `Error`. `from()` simply assigns the supplied parts without those checks, so use it for values you already know are valid.

`year`, `month` and `day` remain writable public fields, not a frozen value. Assigning them directly does not run calendar validation; parse the new date string instead when changing a date.

`fromDate()` reads the host's local year, month and day, not the UTC parts. Choose it when that local calendar date is what you need; the host's zone is not necessarily the zone of the person receiving the result.

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

If a date picker needs a JavaScript `Date`, `toDate()` passes the calendar parts to its local-time constructor. In the ordinary date below, `getDate()` reads back the 12th. This is not a UTC or arbitrary-time-zone conversion, and JavaScript `Date` normalization rules still apply.

```typescript
import { DateOnly } from '@cratis/fundamentals';

const date = DateOnly.parse('2026-05-12');
const asDate = date.toDate();
console.log(asDate.getDate()); // 12
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

For an invoice response, give `@field` the `DateOnly` constructor so `JsonSerializer.deserialize()` rebuilds `issued` as an instance. The built-in `DateOnlyJsonConverter` handles the date string; the `day` access below then reads its calendar part:

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

You can also send a date directly with native JSON serialization. `DateOnly.toJSON()` returns its date string, so `JSON.stringify({ issued: DateOnly.from(2026, 5, 12) })` produces `{"issued":"2026-05-12"}`, not a component object.

Reading that JSON with `JSON.parse()` leaves `issued` as a string. Use `DateOnly.parse()` to restore a scalar date, or the decorated model above when you need the whole response restored. [JsonSerializer](./serialization/json_serializer.md) explains that model boundary.

## Related

- [TimeOnly](./time_only.md) — a time of day with no date and no time zone
- [JsonSerializer](./serialization/json_serializer.md) — how types are converted on the way in and out
- [@field Decorator](./serialization/field_decorator.md) — declaring the runtime type of a field
