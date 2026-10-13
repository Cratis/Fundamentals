---
title: Testing generated values
description: Pin generated values by supplying them, fix the clock with a GenerationContext, and assert on shape when you cannot pin.
---

Use this page when a spec touches a value that is random by design. The approach: put the value in, don't seed the generator.
Production randomness stays secure, and the spec stays repeatable.

## Pin by value

Construct the concept from a known value and pass it to the behavior under test. A concept is a record, so equality compares the wrapped value:

```csharp
var customerId = new CustomerId("cus_01j0dy9j4nej4saxp79mmcqts7");
```

There is no seeded `Random`, injectable generator or seed setting, and a spec should not look for one. Frameworks such as Arc let a spec supply
a generated value by name in place of the real one; use that framework's guidance when you test through it.

## Test that a concept honors the contract

A generic spec proves the concept implements the contract and returns its own type:

```csharp
using Cratis.Concepts;
using Cratis.Specifications;
using Xunit;

namespace Shop.Specs;

public class when_creating_an_invite_code_through_the_contract : Specification
{
    InviteCode _result;

    void Because() => _result = Create<InviteCode>();

    [Fact] void should_have_ten_characters() => _result.Value.Length.ShouldEqual(10);
    [Fact] void should_use_only_the_alphabet() => _result.Value.All("23456789ABCDEFGHJKLMNPQRSTUVWXYZ".Contains).ShouldBeTrue();

    static T Create<T>() where T : IGeneratable<T> => T.New();
}
```

## Pin the clock

For a concept that implements `IGeneratableInContext<TSelf>`, build the context yourself. The time part is then fixed and the random part is not:

```csharp
var received = new DateTimeOffset(2024, 6, 15, 12, 34, 56, 789, TimeSpan.Zero);
var context = new GenerationContext(received, GenerationContext.DefaultTenant);
var orderId = OrderId.New(context);
```

For that `received` value, the characters at positions 6 to 15 of an `order_` TypeID are always `01j0dy9j4n`. Assert that part, and assert
that two ids from one context are different:

```csharp
orderId.Value[6..16].ShouldEqual("01j0dy9j4n");
```

Do not build `new GenerationContext()` in a spec: its `Now` is `default` and the generation throws `MissingGenerationTime`.

## Assert shape when you cannot pin

When a value is generated deep inside the code under test, assert what is guaranteed:

| Property | Assertion |
| --- | --- |
| Length | `Ulid()` is 26 characters; `NanoId()` is 21; a TypeID suffix is 26 |
| Alphabet | Every character is in the alphabet you passed, or in the encoding's alphabet |
| Prefix | A TypeID starts with `prefix_` |
| Time | The embedded time falls between a `before` and an `after` you capture around the call |
| Distinctness | Two calls return different values (not a statistical test) |

Do not assert an exact random value, and do not generate ten thousand ids to prove uniqueness.

## Test the failures

Invalid arguments throw typed exceptions; `Catch.Exception` from `Cratis.Specifications` captures them:

```csharp
Exception error = Catch.Exception(() => GenerateValue.NanoId(0));
error.ShouldBeOfExactType<InvalidNanoIdSize>();
```

## Next steps

- [Reference](reference.md) lists every exception and its trigger.
- [Tutorial: generate your first values](tutorial.md) builds the concepts used here.
