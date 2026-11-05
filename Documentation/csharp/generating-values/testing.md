---
title: Cratis Fundamentals deterministic value testing
description: Make C# specs repeatable by supplying known concepts, fixing generation time, and asserting only the guaranteed shape of random values.
---

To test behavior that uses generated values, supply a known concept instead of calling its random generator.
Cratis Fundamentals does not provide a seed setting for `GenerateValue`. When you test generation itself,
fix the inputs you control and assert only what the strategy guarantees.

## Pin by value

For a behavior test, construct the concept from a known value and pass it to the code under test. This example
uses the string-backed `CustomerId` from [step 2 of the tutorial](tutorial.md#step-2-a-public-id-that-sorts-by-age).
As a record, the concept compares its wrapped value for equality:

```csharp
var customerId = new CustomerId("cus_01j0dy9j4nej4saxp79mmcqts7");
```

The behavior now receives the same identifier on every run, without changing production generation.
When you test through Cratis Arc's command pipeline, use its named generated-value pins instead of calling
`New()` in the spec.

Input-driven strategies need no pin: assert the complete expected `UuidV5` value for a fixed namespace and name,
or the exact `Sha256` digest for fixed text. [SHA-256 digests](sha-256.md) covers the text and comparison rules.

## Test that a concept honors the contract

Use a generic call to check that the concept can be created through `IGeneratable<T>`. The compiler checks the
contract; the assertions check the generated value's length and alphabet. This spec uses `InviteCode` from
[step 3 of the tutorial](tutorial.md#step-3-a-short-invite-code) and the `Cratis.Specifications.XUnit` package:

```csharp
using Cratis.Concepts;
using Cratis.Specifications;
using Xunit;

namespace Shop.Specs;

public class when_creating_an_invite_code_through_the_contract : Specification
{
    InviteCode _result = null!;

    void Because() => _result = Create<InviteCode>();

    [Fact] void should_have_ten_characters() => _result.Value.Length.ShouldEqual(10);
    [Fact] void should_use_only_the_alphabet() => _result.Value.All("23456789ABCDEFGHJKLMNPQRSTUVWXYZ".Contains).ShouldBeTrue();

    static T Create<T>() where T : IGeneratable<T> => T.New();
}
```

## Pin the clock

For a concept that implements `IGeneratableInContext<TSelf>`, supply a `GenerationContext` with a fixed instant.
That fixes the embedded time, not the random part of the identifier. This fragment uses `OrderId` from
[step 5 of the tutorial](tutorial.md#step-5-an-order-id-that-follows-the-operation). Keep the `using` directives
at the top of the spec file and place the statements in its `Because` method:

```csharp
using Cratis.Concepts;
using Cratis.Specifications;

var received = new DateTimeOffset(2024, 6, 15, 12, 34, 56, 789, TimeSpan.Zero);
var context = new GenerationContext(received, GenerationContext.DefaultTenant);
var first = OrderId.New(context);
var second = OrderId.New(context);

first.Value[6..16].ShouldEqual("01j0dy9j4n");
second.Value[6..16].ShouldEqual(first.Value[6..16]);
```

For that `received` value, the characters at positions 6 to 15 of an `order_` TypeID are `01j0dy9j4n`.
The first assertion checks the encoded time; the second checks that both calls use the same time. The remainder
is random. Do not assert that two identifiers differ, because random generation does not guarantee uniqueness.

Supply the instant explicitly. A default `new GenerationContext()` has a `default` `Now`, so the time-dependent
generation shown here throws `MissingGenerationTime`.

## Assert shape when you cannot pin

When you cannot supply a known value, check the properties the strategy guarantees. The same checks are useful
when testing a concept's `New()` implementation:

| Property | Assertion |
| --- | --- |
| Length | `Ulid()` is 26 characters; `NanoId()` is 21; a TypeID suffix is 26 |
| Alphabet | Every character is in the alphabet you passed, or in the encoding's alphabet |
| Prefix | A TypeID starts with `prefix_` |
| Pattern | Literals, alphabets and repeat bounds match the declared format; pinned UTC dates match the context |
| Check digits | `GenerateValue.HasValidChecksum(value, Checksum.Luhn)` is true for a Luhn-generated value |
| Integer | The returned `long` is between both inclusive bounds |
| Time | The embedded time falls between a `before` and an `after` you capture around the call |

Do not assert an exact random value, and do not generate ten thousand ids to prove uniqueness.
For [patterns with dates](patterns.md), pin the `GenerationContext` and assert only the date and shape, not the random payload.
A date-bearing pattern without context throws rather than consulting the wall clock.

## Test the failures

Invalid arguments throw typed exceptions; `Catch.Exception` from `Cratis.Specifications` captures them:

```csharp
using Cratis.Concepts;
using Cratis.Specifications;

Exception error = Catch.Exception(() => GenerateValue.NanoId(0));
error.ShouldBeOfExactType<InvalidNanoIdSize>();
```

## Next steps

Use the [Reference](reference.md) to choose the exception cases your concept should reject.
The [tutorial](tutorial.md) builds the concepts used here and shows a complete pinned-time spec.
