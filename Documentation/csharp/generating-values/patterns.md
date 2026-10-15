---
title: Patterns and checksums
description: Generate readable reference formats with UTC dates, exact uniform sampling and optional check digits.
---

A reference read over the phone needs a recognizable format, not a UUID. `Pattern` keeps the literal text, alphabet,
repetition bounds and check digits together, so the concept owns the complete format. For an ordinary entity identity,
prefer [UUIDs or another high-entropy strategy](choosing-a-strategy.md).

## Define the format

```csharp
using Cratis.Concepts;

public record OrderNumber(string Value) : ConceptAs<string>(Value), IGeneratable<OrderNumber>
{
    public static OrderNumber New() => new(GenerateValue.Pattern("ORD-[A-HJ-NP-Z2-9]{4}-#{4,6}"));
}
```

The result has four characters without easily confused letters, followed by four to six digits, such as `ORD-A3K8-12345`.
Generation does not guarantee uniqueness: protect this short reference with a unique constraint or a freshness check.

## Pattern language

This is a finite pattern language, **not a regular expression**.

| Form | Meaning |
| --- | --- |
| `#` | One ASCII digit, `0`–`9` |
| `?` | One uppercase ASCII letter, `A`–`Z` |
| `*` | One digit or uppercase letter, `0`–`9`, `A`–`Z` |
| `[ABC]`, `[A-F0-9]` | A positive ASCII class; duplicate characters collapse |
| `{n}`, `{m,n}` | Repeat the preceding token, with `1 ≤ m ≤ n ≤ 64` |
| `{yyyy}`, `{yy}`, `{MM}`, `{dd}` | A UTC date segment from the generation context |
| Other printable ASCII | Literal text |

Outside classes, escape reserved characters `# ? * [ ] { } \` with a backslash. Inside classes, the escapes are
`\]`, `\-` and `\\`; an unescaped opening bracket or hyphen is not a literal class member. Use a C# verbatim string
when you want the pattern's backslashes to remain unchanged: `GenerateValue.Pattern(@"\#[AB]")` produces `#A` or `#B`.

Every pattern needs a random token, though a singleton class such as `[A]` has only one output. Empty or negated classes,
descending ranges, unsupported escapes, orphan repeats and non-printable or non-ASCII characters throw `InvalidValuePattern`.
Its message includes the pattern and the rejection reason.

## Fix dates to the operation

Date-bearing patterns require the context overload. Capture one context for the operation; every segment uses its UTC date.

```csharp
using Cratis.Concepts;

public record InvoiceReference(string Value) : ConceptAs<string>(Value), IGeneratableInContext<InvoiceReference>
{
    public static InvoiceReference New(GenerationContext context) =>
        new(GenerateValue.Pattern("INV-{yyyy}{MM}{dd}-#{7}", context, Checksum.Luhn));
}
```

At `2024-03-01T00:30:00+01:00`, the date portion is `20240229`, not `20240301`. A year always has four digits for `yyyy`
and two for `yy`. Dates before 1970 are supported. The tenant does not affect this strategy.

Calling a date-bearing pattern without a context throws `PatternRequiresGenerationContext`; it never reads today's date.
Passing a context whose `Now` is `default` throws `MissingGenerationTime`, even if the pattern has no date segments.

## Append and validate check digits

Pass `Checksum.Luhn`, `Checksum.Mod11` or `Checksum.Mod97` to append check digits. All ASCII digits in the payload contribute,
including digits in literals and dates. Nondigits remain in the result but do not contribute to the arithmetic.
An empty digit payload counts as zero.

| Algorithm | Suffix | Calculation |
| --- | --- | --- |
| Luhn | One digit | Right-to-left Luhn, doubling the rightmost payload digit |
| Mod11 | One digit | Weights 2–7 cycle from the right; `(11 - sum % 11) % 11` |
| Mod97 | Two digits, zero-padded | ISO 7064 MOD 97-10: `98 - ((payload % 97) * 100 % 97)` |

Mod11 excludes payloads whose check would be 10 **before sampling**. For example, `000000006-[AB]` with Mod11 has no
admissible output and throws `EmptyPatternSupport`. The generator does not redraw until something happens to work.

Validate values you receive or read from storage explicitly:

```csharp
using Cratis.Concepts;

bool valid = GenerateValue.HasValidChecksum("79927398713", Checksum.Luhn);
```

Here `valid` is `true`. Validation requires printable ASCII and exact trailing check digits. It rejects malformed suffixes,
Mod11 output 10 and incorrect arithmetic. `Checksum.None` or an undefined algorithm throws `UnknownChecksum`; null throws
`ArgumentNullException`. A checksum detects some transcription mistakes; it is neither authentication nor a uniqueness check.

## Uniformity and limits

Every distinct admissible string has the same probability, even when several repeat decompositions produce it.
For `[AB]{1,3}[AB]{1,3}`, strings of lengths two through six each appear once in the support; a three-character string
is not counted twice because it has two possible decompositions. Longer lengths collectively have more strings and therefore
more probability. This is not uniform selection of a length followed by uniform characters.

The runtime counts completions with exact integers, draws a cryptographically secure index and maps that index to one value.
Check digits add no randomness. Date segments are fixed literals and add no entropy. Successful parsed patterns and counted
supports use bounded, thread-safe process-local caches; the UTC date is part of the support key for date-bearing patterns.

Limits are 256 final characters **including check digits**, 64 repetitions per token, 2,048 automaton character positions,
10,000 distinct DFA/product states and 100,000,000 construction/counting operations. Exceeding a budget throws
`InvalidValuePattern` rather than silently changing the format or falling back to another strategy.

Below 64 bits of entropy, use a unique constraint or freshness protection. Short readable references and random ticket numbers
are poor substitutes for high-entropy entity identities, credentials or gapless legal invoice numbers.

## Random integer references

For a numeric ticket rather than a text format, `GenerateValue.Integer(100000, 999999)` returns an unbiased `long` in that
inclusive range. Equal bounds return that value; inverted bounds throw `InvalidIntegerRange`. The complete
`long.MinValue` through `long.MaxValue` range is supported without overflow. See the [reference](reference.md) for signatures.
