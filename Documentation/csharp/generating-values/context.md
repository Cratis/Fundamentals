---
title: Context-dependent values
description: Capture one time and tenant per operation with GenerationContext and generate values from it.
---

Use `IGeneratableInContext<TSelf>` when a value must reflect the operation that creates it, not the moment each call happens to run.
A command that creates an order id, a log id and a receipt id should stamp all three with the instant the command was received.

## Capture one context per operation

`GenerationContext` is a `readonly record struct` with two members:

| Member | Meaning |
| --- | --- |
| `Now` | The `DateTimeOffset` the operation was received |
| `Tenant` | A portable tenant identifier string |

`GenerationContext.DefaultTenant` is `"00000000-0000-0000-0000-000000000000"`. Use it when there is no named tenant.
Named tenants use their identifier, not a display name. An empty string does not mean the default tenant.

```csharp
using Cratis.Concepts;

var context = new GenerationContext(DateTimeOffset.UtcNow, GenerationContext.DefaultTenant);
```

Capture it once and pass the same value to every factory. Calling the clock separately for each value gives one operation several creation times.

:::note
A framework that runs your operation, such as Arc, is responsible for supplying the context to a concept's `New(GenerationContext)`.
In your own code, or in a spec, you build it yourself.
:::

## Declare the contract on the concept

```csharp
using Cratis.Concepts;

public record OrderId(string Value) : ConceptAs<string>(Value), IGeneratableInContext<OrderId>
{
    public static OrderId New(GenerationContext context) => new(GenerateValue.TypeId("order", context));
}
```

`UuidV7(context)`, `Ulid(context)` and `TypeId(prefix, context)` all use `context.Now`. They do not read `Tenant`. When tenant scope is part of
your value, read it in the concept:

```csharp
using Cratis.Concepts;

public record TenantOrderId(string Value) : ConceptAs<string>(Value), IGeneratableInContext<TenantOrderId>
{
    public static TenantOrderId New(GenerationContext context) => new($"{context.Tenant}-{GenerateValue.Ulid(context)}");
}
```

This is a domain-specific string, not a TypeID. It is rarely needed because each tenant already has its own namespace.

## Call it from generic code

```csharp
static T Create<T>(GenerationContext context) where T : IGeneratableInContext<T> => T.New(context);
```

## Rules that bite

- **Both interfaces: the context form wins.** If a type implements `IGeneratable<T>` and `IGeneratableInContext<T>`, callers use the context form.
  The interfaces do not dispatch or fall back to each other.
- **A `default` context is a bug.** A context with `Now` equal to `default` throws `MissingGenerationTime`; it does not fall back to the current time.
  This usually means a framework forgot to supply the context, or a spec built `new GenerationContext()`.
- **Time before the Unix epoch throws.** `GenerationTimeBeforeUnixEpoch` is thrown when `Now` is earlier than 1970-01-01T00:00:00Z.
- **Do not add the context to every concept.** If a value does not depend on time or tenant, keep `IGeneratable<TSelf>`.

```text
MissingGenerationTime: A generation context must contain the time the operation was received.
GenerationTimeBeforeUnixEpoch: Generation time '1969-12-31T00:00:00.0000000+00:00' must be on or after the Unix epoch.
```

## Next steps

- Pin the context in a spec: [Testing generated values](testing.md).
- Exact signatures: [Reference](reference.md).
