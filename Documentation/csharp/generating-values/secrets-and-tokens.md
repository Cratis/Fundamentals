---
title: Secrets and tokens
description: Generate tokens, reset links, API keys and key material with enough entropy, and keep them out of logs and event streams.
---

Use this page when the value grants access: a reset link, an invitation token, an API key or a TOTP secret. An id that is merely hard to guess
is not a credential. The rules are short: enough random bits, the right strategy, and no trail.

## Generate it

| Need | Code | Bits |
| --- | --- | --- |
| API key, token, reset link | `GenerateValue.Bytes(32)` | 256 |
| Hex token for a header or URL | `GenerateValue.Bytes(32, ValueEncoding.Hex)` | 256 |
| TOTP secret, key material | `GenerateValue.Bytes(20, ValueEncoding.Base32)` | 160 |
| URL-safe token, default alphabet | `GenerateValue.NanoId(32)` | 192 |

- Use at least 128 random bits. `Bytes(16)` is the floor; `Bytes(32)` is the default to reach for.
- Use only `Bytes` or `NanoId` for secrets. A UUID v4 has 122 bits and is not designed as a token. UUID v7, ULID and TypeID expose their creation time
  and carry fewer random bits.
- Encoding changes the representation, not the entropy: 32 bytes are 256 bits as base64url or as hex.
- A short, typeable code such as an invitation code is not a secret. Treat it as an identifier with a uniqueness check and an expiry.

```csharp
using Cratis.Concepts;

public record ApiKey(string Value) : ConceptAs<string>(Value), IGeneratable<ApiKey>
{
    public static ApiKey New() => new(GenerateValue.Bytes(32));
}
```

## Mark the concept so it is not recorded

Generation is not secret storage. In an Arc application with Chronicle, the values of a command's properties are written to the causation chain
of every event the command appends. Without a marking, an `ApiKey` property is recorded there in the clear and stays as long as the events do.
`[NotAudited]` comes from the `Cratis.Arc.Chronicle` package. Put it on the concept and the value is left out of the causation chain of every command
that has a property of that type. It also works on a command type, a single property, or a constructor parameter.

```csharp
using Cratis.Arc.Chronicle.Commands;
using Cratis.Concepts;

[NotAudited]
public record ApiKey(string Value) : ConceptAs<string>(Value), IGeneratable<ApiKey>
{
    public static ApiKey New() => new(GenerateValue.Bytes(32));
}
```

`[NotAudited]` has a narrow scope:

| Covered | Not covered |
| --- | --- |
| The command causation chain | The payload of an event: an event that carries the token stores it in the clear |
| | Log lines, exception messages, responses and anything else that prints the value |
| | Encryption: the value is withheld from the chain, not protected |

:::danger[Do not put a raw token in an event]
Events are permanent and replayed by every observer. Store a hash of the token, or a reference to where it lives, not the token itself.
Return the plaintext to the caller once, in the command response, and never write it anywhere else.
:::

`[PII]` is for personal data such as a name or an email address: Chronicle can then manage and erase the value. Do not use it on a secret that is not
personal data. The two markings are not interchangeable.

## Handle it after generation

1. **Show it once.** Return the plaintext to the caller at creation time and never again.
2. **Store a hash.** Keep a hash of the key, in events and in storage, and compare hashes on use, so a leak does not leak working keys.
   `GenerateValue` does not hash for you.
3. **Never log it.** A concept prints its raw value from `ToString()`, so `Console.WriteLine(apiKey)` and `$"{apiKey}"` write the secret.
4. **Expire and rotate.** A token with no lifetime is a permanent credential.

:::caution[Not an authorization check]
A generated value only identifies or proves possession. Whether the caller may do something is decided by your authorization rules, not by the id.
:::

## Next steps

- [Choosing a strategy](choosing-a-strategy.md) for the entropy arithmetic.
- [Reference](reference.md) for the limits of `Bytes` and `NanoId`.
