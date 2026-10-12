---
title: Generating values
description: Give concepts a compile-time generation contract and choose secure random or time-ordered UUIDs.
---

An ad-hoc `New()` method is easy to call when you know the concept type, but generic code cannot rely on it without a contract.
Random identifiers also make specs hard to repeat if you assert on whichever value a run happens to produce.
Rolling your own generator with `Random` weakens unpredictability.

Fundamentals separates these concerns: `IGeneratable<TSelf>` declares that a type creates its own values,
while `GenerateValue` supplies cryptographically secure UUIDs and strings on .NET 8, 9, and 10.
When generation depends on the operation's clock or tenant, use `IGeneratableInContext<TSelf>`.
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
The contract also works for string-backed concepts. `CorrelationId` implements `IGeneratable<CorrelationId>`
and uses `GenerateValue.Uuid()`.

## Choose a generation strategy

Start with `Uuid()` for identifiers that need no special string shape or ordering. Choose another strategy for a concrete requirement:

| Strategy | Shape | Random bits | Time-ordered? | When to choose |
| --- | --- | --- | --- | --- |
| `Uuid()` | RFC 9562 version 4 `Guid` | 122 | No | Default random identifiers; no creation time encoded |
| `UuidV7()` | RFC 9562 version 7 `Guid` | 74 | By timestamp | Database index locality with UUID storage |
| `Ulid()` | 26 uppercase Crockford base32 characters | 80 | By timestamp | Sortable string identifiers |
| `TypeId(prefix)` | Prefix + `_` + 26 lowercase Crockford base32 characters encoding a UUID v7 | 74 | Within one prefix, by timestamp | Readable, typed string identifiers such as `order_…` |
| `NanoId(size, alphabet)` | `size` characters from the chosen alphabet; defaults to 21 from `A-Z`, `a-z`, `0-9`, `_`, `-` | `size × log₂(alphabet length)` | No | Short random string identifiers |
| `Bytes(count, encoding)` | Encoded random bytes; defaults to unpadded base64url | `8 × count` | No | Opaque strings and tokens |

ULIDs use `0123456789ABCDEFGHJKMNPQRSTVWXYZ`; TypeID suffixes use its lowercase equivalent.
Both encode 128 bits as 26 characters with two leading zero padding bits, so the first character is `0`–`7`.
A ULID contains 48-bit big-endian Unix-epoch milliseconds followed by 80 random bits.
A TypeID suffix contains the UUID v7's 16 big-endian bytes.

A TypeID prefix is empty or contains 1–63 lowercase letters and underscores, starting and ending with a letter.
An empty prefix produces only the suffix, without `_`. Null or invalid prefixes throw `InvalidTypeIdPrefix`.
NanoID sizes are 1–256; alphabets must contain 2–256 distinct printable ASCII characters (`0x20`–`0x7E`).
Invalid inputs throw `InvalidNanoIdSize` or `InvalidNanoIdAlphabet`. Null selects the default alphabet.

`Bytes` accepts 1–512 bytes and a `ValueEncoding`: `Base64Url` uses RFC 4648 URL-safe base64 without padding,
`Hex` uses lowercase hexadecimal, and `Base32` uses RFC 4648 uppercase `A-Z2-7` without padding.
Invalid counts throw `InvalidByteCount`; undefined encodings throw `UnknownValueEncoding`.
For example, these excerpts need a `Cratis.Concepts` import:

```csharp
var shortId = GenerateValue.NanoId(12);
var opaqueToken = GenerateValue.Bytes(32);
var hexToken = GenerateValue.Bytes(32, ValueEncoding.Hex);
```

The tokens each contain 256 random bits; encoding changes their representation, not their entropy.
Token generation is not secret storage: a secret concept needs the relevant `[NotAudited]`/`[Encrypted]` protection
from its host integration, and should store a hash rather than the plaintext token.
Do not use a UUID as an authentication token.

Prefix and case modifiers need no extra generation API. Apply ordinary string operations when your domain requires a different presentation:

```csharp
var customerId = $"cus_{GenerateValue.Ulid().ToLowerInvariant()}";
```

This excerpt assumes a `Cratis.Concepts` import. The result is a custom-prefixed lowercase ULID, not a TypeID:
a TypeID suffix encodes a UUID v7, not ULID bytes.

UUID v7, ULID, and TypeID expose creation time. Choose NanoID or encoded bytes when you need an opaque string.
Time-ordered values are **not monotonic** within one millisecond: random suffixes do not preserve call order.
Clock adjustments can move timestamps backward, and clocks on different machines need not agree.
Check how your database stores and compares UUIDs before relying on UUID v7's canonical string or big-endian byte order for index locality.
None of these strategies guarantees uniqueness. Below 64 random bits, a 1% collision risk arrives at surprisingly modest volumes:
the birthday effect makes the number of values that matters grow roughly with the square root of the value space.
Size identifiers for their total expected population, not just one request's volume.

## Carry the operation's clock and tenant

Calling the clock separately for every value can give one command several different creation times.
Capture one instant when the operation is received, then pass that same `GenerationContext` to each factory:

```csharp
using Cratis.Concepts;

public record OrderId(string Value) : ConceptAs<string>(Value), IGeneratableInContext<OrderId>
{
    public static OrderId New(GenerationContext context) => new(GenerateValue.TypeId("order", context));
}
```

`OrderId.New(context)` puts `context.Now` in the UUID v7 suffix. `UuidV7(context)` and `Ulid(context)`
also use that captured instant. The built-in strategies do not read `Tenant`; your concept can use it when tenant scope is part of its value:

```csharp
using Cratis.Concepts;

public record TenantOrderId(string Value) : ConceptAs<string>(Value), IGeneratableInContext<TenantOrderId>
{
    public static TenantOrderId New(GenerationContext context) => new($"{context.Tenant}_{GenerateValue.Ulid(context)}");
}
```

The tenant prefix and captured time now travel together through the static contract. This is a domain-specific string, not a TypeID prefix.
`GenerationContext.Tenant` is a portable string identifier: `GenerationContext.DefaultTenant`
is `"00000000-0000-0000-0000-000000000000"` for the default tenant; named tenants use their identifier.
Do not substitute a display name or infer that an empty string means the default tenant.

For generic callers, the corresponding factory excerpt is:

```csharp
static T Create<T>(GenerationContext context)
    where T : IGeneratableInContext<T> => T.New(context);
```

If a type implements **both** interfaces, callers use the context form. The interfaces do not perform dispatch or supply a fallback.
A default context is a defect: each built-in context overload throws `MissingGenerationTime` when `Now` is default,
not a request to use the current time. A time before the Unix epoch throws `GenerationTimeBeforeUnixEpoch`.

Use the context contract only when generation depends on the clock or tenant.
A context-free random identifier can keep `IGeneratable<TSelf>`; there is no reason to pass unused operation metadata.

## When not to generate

- A client-supplied identifier is an input. Preserve it rather than generating a replacement.
- Random generation does not provide idempotency. If the same input must produce the same identifier,
  use a deterministic identifier strategy or persist and reuse the first identifier.
- A short random identifier is not automatically a safe token. Choose enough entropy and protect secret storage and audit paths separately.

## Randomness and runtime support

Every strategy uses a cryptographically secure pseudorandom number generator (CSPRNG).
NanoID draws uniformly from its alphabet with `RandomNumberGenerator.GetItems<char>`.
There is no seeded `Random`, injectable generator, seed configuration, or within-millisecond counter.

On .NET 9 and later, UUID v7 generation delegates to `Guid.CreateVersion7` with the captured time.
On .NET 8, both overloads share a polyfill that writes six timestamp bytes, sets version 7 and the RFC variant,
and keeps 74 bits from `RandomNumberGenerator`. UUID v4 keeps 122 random bits after setting its format bits.

The helpers and constrained static interface calls use no reflection or dynamic code.
They are safe for Native AOT and trimming; this does not imply that every unrelated concept discovery or serialization path is.

## Test with a known value or clock

Generated values are intentionally random. Pin an identifier in your spec's setup and pass it to the behavior under test:

```csharp
var authorId = new AuthorId(Guid.Parse("5f4a6767-5da5-41f2-b888-7801d4ec8258"));
```

This setup excerpt assumes the `AuthorId` definition above and a `System` import.
Assert against that same value after the behavior runs. Do not seed a generator to make production IDs predictable.
When testing a context-dependent factory, pin the clock through the context instead:

```csharp
var context = new GenerationContext(
    DateTimeOffset.Parse("2024-06-15T12:34:56.789Z"),
    GenerationContext.DefaultTenant);
var orderId = OrderId.New(context);
```

This excerpt assumes the `OrderId` definition above and `System` and `Cratis.Concepts` imports.
The timestamp is fixed; the random suffix is not. Assert the timestamp and tenant contract, not an exact random value.
For generation itself, check shape, version, variant, timestamp bounds, and distinct sample values.
Arc's `[GeneratedValue]` integration pins generated values by name in specs; use that integration's guidance when testing an Arc command.
