---
title: Generated values reference
description: Every public type and member for generating values, with output shapes, random bits, limits, exceptions and encodings.
---

All types are in the `Cratis.Concepts` namespace of `Cratis.Fundamentals`.

## Contracts

| Type | Member | Meaning |
| --- | --- | --- |
| `IGeneratable<TSelf>` where `TSelf : IGeneratable<TSelf>` | `static abstract TSelf New()` | Creates a value with the type's own strategy |
| `IGeneratableInContext<TSelf>` where `TSelf : IGeneratableInContext<TSelf>` | `static abstract TSelf New(GenerationContext context)` | Creates a value from the operation's captured time and tenant |

If a type implements both, callers use the context form. Neither interface dispatches to the other.

## GenerationContext

`public readonly record struct GenerationContext(DateTimeOffset Now, string Tenant)`

| Member | Value |
| --- | --- |
| `Now` | The time the operation was received |
| `Tenant` | A portable tenant identifier string |
| `const string DefaultTenant` | `"00000000-0000-0000-0000-000000000000"` |

## GenerateValue

`public static partial class GenerateValue`. Every method uses a cryptographically secure generator.

| Signature | Output shape | Random bits | Time-ordered | Limits and exceptions |
| --- | --- | --- | --- | --- |
| `Guid Uuid()` | RFC 9562 version 4, RFC variant | 122 | No | None |
| `Guid UuidV7()` | RFC 9562 version 7 using the current UTC time | 74 | Yes | None |
| `Guid UuidV7(GenerationContext context)` | Version 7 using `context.Now` | 74 | Yes | `MissingGenerationTime`, `GenerationTimeBeforeUnixEpoch` |
| `string Ulid()` | 26 uppercase Crockford base32 characters; first character `0` to `7` | 80 | Yes | None |
| `string Ulid(GenerationContext context)` | Same, using `context.Now` | 80 | Yes | `MissingGenerationTime`, `GenerationTimeBeforeUnixEpoch` |
| `string TypeId(string prefix)` | `prefix` + `_` + 26 lowercase Crockford characters; only the suffix for an empty prefix | 74 | Yes | `InvalidTypeIdPrefix` |
| `string TypeId(string prefix, GenerationContext context)` | Same, using `context.Now` | 74 | Yes | `InvalidTypeIdPrefix`, `MissingGenerationTime`, `GenerationTimeBeforeUnixEpoch` |
| `string NanoId(int size = 21, string? alphabet = null)` | `size` characters drawn uniformly from the alphabet | `size × log₂(alphabet length)` | No | `InvalidNanoIdSize`, `InvalidNanoIdAlphabet` |
| `string Bytes(int count, ValueEncoding encoding = ValueEncoding.Base64Url)` | Encoded random bytes, no padding | `8 × count` | No | `InvalidByteCount`, `UnknownValueEncoding` |

Argument limits:

- **TypeID prefix:** empty, or 1 to 63 lowercase letters (`a`–`z`) and underscores, starting and ending with a letter. A null or invalid prefix throws `InvalidTypeIdPrefix`.
- **NanoID size:** 1 to 256. The default is 21.
- **NanoID alphabet:** 2 to 256 distinct printable ASCII characters (`0x20` to `0x7E`). Null selects the default alphabet
  `ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-`, 64 characters, so the default 21 characters carry 126 bits.
- **Bytes count:** 1 to 512.
- **Context time:** `Now` equal to `default` throws `MissingGenerationTime`; a `Now` before 1970-01-01T00:00:00Z throws `GenerationTimeBeforeUnixEpoch`.

## ValueEncoding

| Member | Value | Output of `Bytes` |
| --- | --- | --- |
| `Base64Url` | 0 | RFC 4648 URL-safe base64 without padding; 32 bytes give 43 characters |
| `Hex` | 1 | Lowercase hexadecimal; 32 bytes give 64 characters |
| `Base32` | 2 | RFC 4648 uppercase `A`–`Z`, `2`–`7` without padding; 20 bytes give 32 characters |

An undefined enum value throws `UnknownValueEncoding`.

## Exceptions

| Exception | Thrown when |
| --- | --- |
| `MissingGenerationTime` | A context overload receives a `default` `Now` |
| `GenerationTimeBeforeUnixEpoch` | A context overload receives a `Now` before the Unix epoch |
| `InvalidTypeIdPrefix` | The TypeID prefix is null or breaks the prefix rule |
| `InvalidNanoIdSize` | The size is outside 1 to 256 |
| `InvalidNanoIdAlphabet` | The alphabet is outside 2 to 256 characters, not distinct, or not printable ASCII |
| `InvalidByteCount` | The count is outside 1 to 512 |
| `UnknownValueEncoding` | The encoding is not a defined `ValueEncoding` |

## Encodings of ULID and TypeID

- ULID and TypeID encode 128 bits as 26 base32 characters. 128 is not a multiple of 5, so two zero bits are added at the start, and the first character is `0` to `7`.
- A ULID is 48 bits of big-endian Unix milliseconds followed by 80 random bits. Its alphabet is `0123456789ABCDEFGHJKMNPQRSTVWXYZ`.
- A TypeID suffix is the 16 big-endian bytes of a UUID v7 in the lowercase alphabet `0123456789abcdefghjkmnpqrstvwxyz`.
- The first 10 characters of either encoding carry the 48-bit timestamp.

## UUID v7 on each runtime

| Runtime | Implementation |
| --- | --- |
| .NET 9 and later | Delegates to `Guid.CreateVersion7` with the supplied time |
| .NET 8 | Built-in polyfill: six timestamp bytes, version 7 and RFC variant bits set, 74 bits from `RandomNumberGenerator` |

Values from both are time-ordered but not monotonic within one millisecond. Clock adjustments can move timestamps backward.

## Randomness and runtime support

- Every strategy uses `RandomNumberGenerator`. NanoID chooses characters with `RandomNumberGenerator.GetItems<char>`, so each character is uniform.
- There is no seeded `Random`, injectable generator, seed configuration or within-millisecond counter.
- The helpers and the static interface calls use no reflection or dynamic code and are safe for Native AOT and trimming.
- Targets: .NET 8, 9 and 10.
