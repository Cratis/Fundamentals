---
title: "Tutorial: generate your first values"
description: Build the ids and codes of a small shop step by step, from a UUID to a pinned-clock spec.
---

You are building a small shop. Customers need ids, orders need ids that carry a time, invitations need short codes you can read out loud,
and integrations need API keys. Today the code is full of `Guid.NewGuid()`. In this tutorial you replace each one with a concept that
generates its own value, and you see a result after every step.

You need the .NET 8 SDK or later. Create a console project and add the package:

```bash
dotnet new console -n Shop
cd Shop
dotnet add package Cratis.Fundamentals
```

The printed values below are samples. Yours differ because the values are random, which is the point.

## Step 1: a customer id that generates itself

Add `CustomerId.cs`:

```csharp
using Cratis.Concepts;

public record CustomerId(Guid Value) : ConceptAs<Guid>(Value), IGeneratable<CustomerId>
{
    public static CustomerId New() => new(GenerateValue.Uuid());
}
```

Replace `Program.cs` with `Console.WriteLine(CustomerId.New());` and run `dotnet run`:

```text
054092b2-48e9-4a54-9bc2-d2e8ccfa04e1
```

`GenerateValue.Uuid()` returns an RFC 9562 version 4 UUID from a cryptographically secure generator. `IGeneratable<CustomerId>` is the
promise that `CustomerId.New()` exists, checked by the compiler. A `ConceptAs<Guid>` prints its inner value, which is why you see a plain UUID.

## Step 2: a public id that sorts by age

Customers will see their id in URLs and invoices, so make it self-describing and sortable. Replace `CustomerId.cs`:

```csharp
using Cratis.Concepts;

public record CustomerId(string Value) : ConceptAs<string>(Value), IGeneratable<CustomerId>
{
    public static CustomerId New() => new($"cus_{GenerateValue.Ulid().ToLowerInvariant()}");
}
```

Replace `Program.cs` to create three ids a few milliseconds apart and print them in sorted order:

```csharp
var ids = new List<CustomerId>();
for (var i = 0; i < 3; i++)
{
    ids.Add(CustomerId.New());
    await Task.Delay(5);
}

foreach (var id in ids.OrderBy(id => id.Value, StringComparer.Ordinal))
{
    Console.WriteLine(id);
}
```

```text
cus_01m4tq3wcw1fn5912rd8rsmgcd
cus_01m4tq3wd2ap7q35madm22wkte
cus_01m4tq3wekqn178wa6k9xeck0x
```

A ULID starts with 48 bits of Unix time in milliseconds, so ids sort by creation time as plain strings. The `cus_` prefix and the lower case are
ordinary string operations; the result is a custom-prefixed ULID, not a TypeID. Two ids created in the same millisecond have no guaranteed order,
and every id reveals when it was created.

:::note
A `cus_` id is easy to read in logs and support tickets. If you want the prefix handled for you, [Choosing a strategy](choosing-a-strategy.md)
shows `TypeId`.
:::

## Step 3: a short invite code

Invitations are typed by people, so remove the characters that look alike. Add `InviteCode.cs`:

```csharp
using Cratis.Concepts;

public record InviteCode(string Value) : ConceptAs<string>(Value), IGeneratable<InviteCode>
{
    const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public static InviteCode New() => new(GenerateValue.NanoId(10, Alphabet));
}
```

The alphabet has 32 characters, so each character carries 5 bits and a 10-character code carries 50. Print three codes, regenerating on a clash:

```csharp
var issued = new HashSet<string>();
for (var i = 0; i < 3; i++)
{
    InviteCode code;
    do
    {
        code = InviteCode.New();
    }
    while (!issued.Add(code.Value));

    Console.WriteLine(code);
}
```

```text
SA65PT7D2G
5GPGMEGTLS
6KSESF9B9G
```

:::caution[50 bits is short]
The chance of any two codes colliding reaches 1% after about 4.8 million codes. That is fine for invitations only because the
loop above checks for a clash. In a real shop that check is a unique index. [Choosing a strategy](choosing-a-strategy.md#how-many-random-bits-you-need)
explains the arithmetic.
:::

## Step 4: an API key

An API key is a secret, so use enough bytes. Add `ApiKey.cs`:

```csharp
using Cratis.Concepts;

public record ApiKey(string Value) : ConceptAs<string>(Value), IGeneratable<ApiKey>
{
    public static ApiKey New() => new(GenerateValue.Bytes(32));
}
```

`Bytes(32)` draws 256 random bits and encodes them as unpadded URL-safe base64, 43 characters:

```csharp
Console.WriteLine(ApiKey.New());
```

```text
gziGp2TEJPHWl6ztCXqiKMj7NVcyO3JhGD7t59Qe204
```

Printing a key is fine in a tutorial and a leak in production: the concept prints its value. In an application that records command
properties, mark the concept so the key never reaches the event log. The attribute comes from Arc's Chronicle integration, not from Fundamentals:

```csharp
using Cratis.Arc.Chronicle.Commands;

[NotAudited]
public record ApiKey(string Value) : ConceptAs<string>(Value), IGeneratable<ApiKey>
{
    public static ApiKey New() => new(GenerateValue.Bytes(32));
}
```

[Secrets and tokens](secrets-and-tokens.md) covers storage, display and logging.

## Step 5: an order id that follows the operation

An order id should carry the time the order was *received*, even if the code takes a few milliseconds to reach the point where it
creates the id. Add `OrderId.cs`:

```csharp
using Cratis.Concepts;

public record OrderId(string Value) : ConceptAs<string>(Value), IGeneratableInContext<OrderId>
{
    public static OrderId New(GenerationContext context) => new(GenerateValue.TypeId("order", context));
}
```

This concept implements `IGeneratableInContext<OrderId>`, so `New` takes a `GenerationContext` holding the captured time and the tenant.
Create two ids from one context and one from a context an hour later:

```csharp
var received = new DateTimeOffset(2024, 6, 15, 12, 34, 56, 789, TimeSpan.Zero);
var context = new GenerationContext(received, GenerationContext.DefaultTenant);

Console.WriteLine(OrderId.New(context));
Console.WriteLine(OrderId.New(context));
Console.WriteLine(OrderId.New(context with { Now = received.AddHours(1) }));
```

```text
order_01j0dy9j4nej4saxp79mmcqts7
order_01j0dy9j4ne2xvhv8rhe3n72ze
order_01j0e1qdrnfvm9xabkp66m09gw
```

The first ten characters after `order_` encode the time. The first two ids share them because they share one `Now`; the third differs.
The remaining characters are random, so the two ids are still different. The built-in strategies read `Now` only, never `Tenant`; a concept can
use the tenant itself, as [Context-dependent values](context.md) shows.

## Step 6: create values from generic code

Frameworks create your values without knowing their types. You can write the same shape in a few lines:

```csharp
static T Create<T>() where T : IGeneratable<T> => T.New();

Console.WriteLine(Create<CustomerId>());
Console.WriteLine(Create<InviteCode>());
```

`T.New()` is a static abstract interface member, so the call is resolved at compile time with no reflection. You get back a `CustomerId`
and an `InviteCode`, not untyped strings. `OrderId` is not accepted by this method, because it implements the context interface:

```csharp
static T Create<T>(GenerationContext context) where T : IGeneratableInContext<T> => T.New(context);
```

## Step 7: pin the clock in a spec

Random values make poor expectations, but the time part of `OrderId` is yours to fix. Create a spec project next to `Shop`:

```bash
dotnet new xunit -n Shop.Specs
cd Shop.Specs
dotnet add package Cratis.Specifications.XUnit
dotnet add reference ../Shop/Shop.csproj
```

Add `when_creating_two_orders_at_a_known_time.cs`:

```csharp
using Cratis.Concepts;
using Cratis.Specifications;
using Xunit;

namespace Shop.Specs;

public class when_creating_two_orders_at_a_known_time : Specification
{
    OrderId _first;
    OrderId _second;

    void Because()
    {
        var received = new DateTimeOffset(2024, 6, 15, 12, 34, 56, 789, TimeSpan.Zero);
        var context = new GenerationContext(received, GenerationContext.DefaultTenant);
        _first = OrderId.New(context);
        _second = OrderId.New(context);
    }

    [Fact] void should_use_the_order_prefix() => _first.Value.StartsWith("order_").ShouldBeTrue();
    [Fact] void should_encode_the_received_time() => _first.Value[6..16].ShouldEqual("01j0dy9j4n");
    [Fact] void should_share_the_time_between_orders() => _second.Value[6..16].ShouldEqual(_first.Value[6..16]);
    [Fact] void should_still_create_distinct_ids() => (_first == _second).ShouldBeFalse();
}
```

Run `dotnet test`. All four pass on every run: the spec asserts the parts you control and does not assert the random part.

## What you built

| Concept | Strategy | Why |
| --- | --- | --- |
| `CustomerId` | `cus_` + lowercase `Ulid()` | Readable, sortable public id |
| `InviteCode` | `NanoId(10, alphabet)` | Short, typeable, needs a uniqueness check |
| `ApiKey` | `Bytes(32)` | 256 random bits, a secret |
| `OrderId` | `TypeId("order", context)` | Time follows the operation |

## Next steps

- Choose strategies for your own ids with [Choosing a strategy](choosing-a-strategy.md).
- Learn the rules for the context in [Context-dependent values](context.md).
- Handle your API keys safely with [Secrets and tokens](secrets-and-tokens.md).
