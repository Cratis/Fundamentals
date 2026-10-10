---
title: Generating values
description: Give concepts a compile-time generation contract and choose secure random or time-ordered UUIDs.
---

An ad-hoc `New()` method is easy to call when you know the concept type, but generic code cannot rely on it without a contract.
Random identifiers also make specs hard to repeat if you assert on whichever value a run happens to produce.
Rolling your own generator with `Random` weakens unpredictability, and .NET 8 has no built-in UUID v7 generator.

Fundamentals separates these concerns: `IGeneratable<TSelf>` declares that a type creates its own values,
while `GenerateValue` supplies cryptographically secure UUID v4 and v7 generation on .NET 8, 9, and 10.
Keep test expectations stable by supplying a known value, not by replacing production randomness.

## Let the concept own generation

With a reference to `Cratis.Fundamentals`, define a [concept](concepts.md) that implements the static contract:

```csharp
using System;
using Cratis.Concepts;

public record AuthorId(Guid Value) : ConceptAs<Guid>(Value), IGeneratable<AuthorId>
{
    public static AuthorId New() => new(GenerateValue.Uuid());
}
```

`AuthorId.New()` returns a new random identifier. The interface requires a `static abstract TSelf New()` method,
so constrained generic code can call it without reflection. For example, this method can live in your own factory:

```csharp
static T Create<T>()
    where T : IGeneratable<T> => T.New();
```

Calling `Create<AuthorId>()` uses the concept's strategy and returns an `AuthorId`, not an untyped `Guid`.
The interface is not limited to GUID-backed concepts; the UUID helpers are.
`CorrelationId` also implements `IGeneratable<CorrelationId>` and uses `GenerateValue.Uuid()`.

## Choose random or time-ordered UUIDs

Both helpers return a `Guid` using the RFC 9562 variant:

| Helper | Contents | Choose it for |
| --- | --- | --- |
| `GenerateValue.Uuid()` | Version 4, with 122 random bits | Random identifiers that do not encode creation time |
| `GenerateValue.UuidV7()` | Version 7, with 48-bit Unix-epoch milliseconds and 74 random bits | Time-ordered identifiers where database index locality matters |

UUID v7 places the timestamp first in the canonical string and big-endian bytes.
Values with a later timestamp sort after values with an earlier timestamp in those representations.
Check how your database stores and compares UUIDs before relying on this for index locality.

Version 7 exposes creation time. Do not choose it when that information must remain private.
Values generated within one millisecond are **not monotonic**: their random suffixes do not preserve call order.
Clock adjustments can also move timestamps backward, and clocks on different machines need not agree.
Neither helper guarantees uniqueness; collisions are extremely unlikely, not impossible.

## When not to generate

- A client-supplied identifier is an input. Preserve it rather than generating a replacement.
- A UUID is an identifier, not a secret or authentication token. Use a dedicated secure token API for credentials.
- Random generation does not provide idempotency. If the same input must produce the same identifier,
  use a deterministic identifier strategy or persist and reuse the first identifier.

## Randomness and runtime support

`GenerateValue.Uuid()` fills 16 bytes with `RandomNumberGenerator`, then sets the version and variant bits.
The remaining 122 bits come from a cryptographically secure pseudorandom number generator (CSPRNG).
`UuidV7()` likewise uses cryptographically secure randomness for its 74 non-timestamp, non-format bits.
There is no seeded `Random`, injectable generator, or seed configuration.

On .NET 9 and later, `UuidV7()` delegates to `Guid.CreateVersion7()`.
On .NET 8, Fundamentals provides a polyfill: it writes the current UTC Unix-epoch milliseconds into six big-endian bytes,
sets version 7 and the RFC variant, and leaves the remaining 74 bits from `RandomNumberGenerator`.
It constructs the GUID with `bigEndian: true`, so the timestamp appears first just as it does with the built-in generator.
The polyfill does not add a within-millisecond counter.

The helpers and constrained static interface calls use no reflection or dynamic code.
They are safe for Native AOT and trimming; this does not imply that every unrelated concept discovery or serialization path is.

## Test with a known value

Generated values are intentionally random. Pin an identifier in your spec's setup and pass it to the behavior under test:

```csharp
var authorId = new AuthorId(Guid.Parse("5f4a6767-5da5-41f2-b888-7801d4ec8258"));
```

This setup excerpt assumes the `AuthorId` definition above and a `System` import.
Assert against that same value after the behavior runs. Do not seed a generator to make production IDs predictable.
When specifying generation itself, assert its contract—version, variant, timestamp bounds, and distinct sample values—rather than an exact UUID.
Arc's `[GeneratedValue]` integration pins generated values by name in specs; use that integration's guidance when testing an Arc command.
