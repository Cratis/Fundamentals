---
title: "Cratis Fundamentals tutorial: generate shop values"
description: Build C# concepts that generate shop identifiers, invitation codes and API keys, then write a spec that fixes the time in an order identifier.
---

In this Cratis Fundamentals tutorial, you will build C# concepts that generate identifiers, invitation codes and
API keys for a small shop. Each concept owns the rule for creating its value, so callers do not repeat that rule.
You will finish with a spec that fixes an order identifier's embedded time while leaving its random part random.

You need the .NET 10 SDK or later. Create a folder for the solution with a console project inside it, and add the package.
Every command in this tutorial runs from that `shop` folder:

```bash
mkdir shop
cd shop
dotnet new console -n Shop
dotnet add Shop package Cratis.Fundamentals
```

You will add the `.cs` files to the `Shop` folder and run the program with `dotnet run --project Shop`. The console template turns on implicit
usings, so `Program.cs` needs no `using` for `System`, `System.Linq`, `System.Collections.Generic` or `System.Threading.Tasks`.

The printed values below illustrate the output format. Your random values will differ.

## Step 1: a customer id that generates itself

Add `CustomerId.cs`:

```csharp
using Cratis.Concepts;

public record CustomerId(Guid Value) : ConceptAs<Guid>(Value), IGeneratable<CustomerId>
{
    public static CustomerId New() => new(GenerateValue.Uuid());
}
```

Replace `Shop/Program.cs` with `Console.WriteLine(CustomerId.New());` and run `dotnet run --project Shop`:

```text
054092b2-48e9-4a54-9bc2-d2e8ccfa04e1
```

`ConceptAs<Guid>` wraps the UUID as a customer identifier, giving it a domain meaning instead of passing a raw
`Guid` through your code. `IGeneratable<CustomerId>` requires the static `New()` method, which the compiler checks.
Here, that method delegates to `GenerateValue.Uuid()` for an RFC 9562 version 4 UUID from a cryptographically secure
generator. The concept's `ToString()` returns its inner value, which is why the console shows a plain UUID.

## Step 2: a public id that sorts by age

For an identifier that appears in URLs and invoices, add a customer prefix and a time-bearing value.
Replace `CustomerId.cs`:

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

A ULID starts with 48 bits of Unix time in milliseconds, so ordinal string sorting follows its embedded time.
The `cus_` prefix and lowercase conversion are ordinary string operations; the result is a custom-prefixed ULID,
not a TypeID. The identifier exposes its creation time. Two values created in the same millisecond have no
guaranteed order, and clock adjustments can affect chronological order.

:::note
The `cus_` prefix identifies the kind of value in a log or support ticket. If you want the generator to handle
the prefix, [Choosing a strategy](choosing-a-strategy.md) shows `TypeId`.
:::

## Step 3: a short invite code

For invitations that people type or read aloud, use a shorter value with an alphabet that omits ambiguous
characters. This code is an identifier, not a bearer credential granting access by itself. Add `InviteCode.cs`:

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
The chance of a collision reaches about 1% after 4.8 million generated codes. This loop checks only the codes
issued during the current run. In a real shop, enforce uniqueness with a unique index, retry a rejected code
and give invitations an expiry. None of those checks turns 50 bits into a high-entropy bearer token.
[Choosing a strategy](choosing-a-strategy.md#how-many-random-bits-you-need) explains the arithmetic.
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

`Bytes(32)` draws 256 random bits and encodes them as 43 characters of unpadded URL-safe base64.
The next line displays a throwaway key so you can inspect that format. Do not use it for a real credential:
`Console.WriteLine` calls the concept's `ToString()` and exposes the raw secret.

```csharp
Console.WriteLine(ApiKey.New());
```

```text
gziGp2TEJPHWl6ztCXqiKMj7NVcyO3JhGD7t59Qe204
```

In an application, return a bearer key once and store only its digest for later verification. Keep the raw key
out of logs and events. Cratis Arc applications using Cratis Chronicle must also exclude it from command
causation, the event metadata that records what caused the event.

The following example is for that Arc application, not this console project. It uses `[NotAudited]` from the
`Cratis.Arc.Chronicle` package, which the tutorial project does not reference:

```csharp
using Cratis.Arc.Chronicle.Commands;
using Cratis.Concepts;

[NotAudited]
public record ApiKey(string Value) : ConceptAs<string>(Value), IGeneratable<ApiKey>
{
    public static ApiKey New() => new(GenerateValue.Bytes(32));
}
```

`[NotAudited]` covers the command causation chain only. It does not protect an event that carries the key, or a log line.
[Secrets and tokens](secrets-and-tokens.md) explains what it covers and how to handle the key.

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
Replace `Shop/Program.cs` to create two ids from one context and one from a context an hour later:

```csharp
using Cratis.Concepts;

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
The remaining characters are random, so the two ids usually differ, but random generation does not guarantee uniqueness. The built-in strategies read `Now` only, never `Tenant`; a concept can
use the tenant itself, as [Context-dependent values](context.md) shows.

## Step 6: create values from generic code

Generic code can ask for a value without knowing the concrete concept's generation strategy. Replace
`Shop/Program.cs` with these helpers and calls:

```csharp
using Cratis.Concepts;

static T Create<T>()
    where T : IGeneratable<T> => T.New();

static T CreateInContext<T>(GenerationContext context)
    where T : IGeneratableInContext<T> => T.New(context);

var context = new GenerationContext(DateTimeOffset.UtcNow, GenerationContext.DefaultTenant);

Console.WriteLine(Create<CustomerId>());
Console.WriteLine(Create<InviteCode>());
Console.WriteLine(CreateInContext<OrderId>(context));
```

The constraints give each helper access to a static abstract `New()` method without reflection. Each call
returns the requested concept: `CustomerId`, `InviteCode` or `OrderId`. `Create<OrderId>()` does not compile
because `OrderId` implements only the context interface; the caller must supply a `GenerationContext`.

## Step 7: pin the clock in a spec

Random values make poor expectations, but the time part of `OrderId` is yours to fix. From the `shop` folder, create a spec project
next to `Shop` and reference it:

```bash
dotnet new xunit -n Shop.Specs
dotnet add Shop.Specs package Cratis.Specifications.XUnit
dotnet add Shop.Specs reference Shop/Shop.csproj
rm Shop.Specs/UnitTest1.cs
```

The `rm` deletes the template's sample test, which the spec project does not need. Add `Shop.Specs/when_creating_two_orders_at_a_known_time.cs`:

```csharp
using Cratis.Concepts;
using Cratis.Specifications;
using Xunit;

namespace Shop.Specs;

public class when_creating_two_orders_at_a_known_time : Specification
{
    OrderId _first = null!;
    OrderId _second = null!;

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
}
```

Run `dotnet test Shop.Specs`. The three facts check the prefix, the encoded received time and the shared time
between calls. They leave the random characters unconstrained, so the expected result does not depend on which
random values the generator produces.

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
