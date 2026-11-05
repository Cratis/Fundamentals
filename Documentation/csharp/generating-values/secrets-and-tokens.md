---
title: Cratis Fundamentals secrets and token handling
description: Generate high-entropy tokens and API keys with Cratis Fundamentals, store bearer-token digests, and keep raw secrets out of logs and events.
---

Use `GenerateValue.Bytes` or `GenerateValue.NanoId` in Cratis Fundamentals when a generated value grants access,
such as an API key or a reset-link token. Generation is only the first step: return a bearer token once, store its
digest for later verification, and keep the raw value out of logs and events. A TOTP secret, used to compute
time-based one-time passwords, needs protected storage from which you can recover the original value.

## Generate it

| Need | Code | Bits |
| --- | --- | --- |
| API key, token, reset link | `GenerateValue.Bytes(32)` | 256 |
| Hex token for a header or URL | `GenerateValue.Bytes(32, ValueEncoding.Hex)` | 256 |
| TOTP secret, key material | `GenerateValue.Bytes(20, ValueEncoding.Base32)` | 160 |
| URL-safe token, default alphabet | `GenerateValue.NanoId(32)` | 192 |

For bearer tokens, use at least 128 random bits: `Bytes(16)` meets that minimum, while `Bytes(32)` gives you 256.
Entropy measures how much uncertainty a random value contains. Encoding changes its representation, not its entropy,
so 32 bytes still carry 256 bits when encoded as base64url or hex.

Of these generation strategies, use only `Bytes` or `NanoId` for secrets. A UUID v4 has 122 random bits;
UUID v7, ULID and TypeID expose their creation time and carry fewer random bits. Short, typeable invitation codes
should serve as identifiers with a uniqueness check and an expiry, not as bearer credentials.

```csharp
using Cratis.Concepts;

public record ApiKey(string Value) : ConceptAs<string>(Value), IGeneratable<ApiKey>
{
    public static ApiKey New() => new(GenerateValue.Bytes(32));
}
```

## Mark the concept so it is not recorded

In a Cratis Arc application using Cratis Chronicle, command property values are recorded in the causation chain:
the metadata that says what caused an event. An unmarked `ApiKey` property can therefore travel with every event
the command appends and remain there for as long as those events do.

`[NotAudited]` comes from the `Cratis.Arc.Chronicle` package, not Fundamentals. Put it on the concept to exclude
command properties of that type from the causation chain. You can also mark a command type, a single property
or its corresponding constructor parameter.

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
An event payload can be retained and replayed long after the command finishes. Store a digest of a bearer token,
or a reference to protected secret storage, instead of the raw secret. Return a bearer token to the caller once
in the command response; do not persist the plaintext.
:::

`[PII]` identifies personal data such as a name or email address for Chronicle's compliance handling. Arc also
excludes those properties from command causation. Use `[NotAudited]` for a secret that is not personal data;
the markings serve different purposes.

## Handle it after generation

For bearer tokens and API keys that callers present back to you, keep only a digest in storage and events.
Use [`GenerateValue.Sha256(token)`](sha-256.md) only for server-generated tokens with at least 128 bits of entropy,
not passwords, PINs or short codes. When a token is presented, hash its text and compare the result with the stored
digest. For direct comparison, use `CryptographicOperations.FixedTimeEquals` over equal-length digest bytes.
Lookup by digest index is not constant-time and is acceptable only for these high-entropy tokens.

A TOTP secret or signing key must remain recoverable because later operations need the original value.
Keep it in protected storage, such as a secret store or encrypted storage, never in an event payload.

Check logging and error paths before using either kind of secret. A concept prints its raw value from `ToString()`,
so `Console.WriteLine(apiKey)` and `$"{apiKey}"` both write the secret. Give tokens a lifetime and a rotation path;
generation and hashing do not expire them for you.

:::caution[Not an authorization check]
A generated value only identifies or proves possession. Whether the caller may do something is decided by your authorization rules, not by the id.
:::

## Next steps

Follow [SHA-256 digests](sha-256.md) to store and compare a bearer-token verifier.
Use [Choosing a strategy](choosing-a-strategy.md) for the entropy arithmetic and the
[Reference](reference.md) for the limits of `Bytes` and `NanoId`.
