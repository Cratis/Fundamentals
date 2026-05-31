# C# (.NET)

`Cratis.Fundamentals` is the [NuGet package](https://www.nuget.org/packages/Cratis.Fundamentals/) the
rest of the stack stands on — strongly-typed domain values, robust serialization, convention-based
dependency injection, and runtime type discovery. You meet it indirectly all the time when you use
Chronicle or Arc, and you can pull it into any .NET project on its own.

The piece you'll reach for first is **concepts**.

## Define your first concept

A `Guid` is a `Guid` is a `Guid` — the compiler can't tell you that you passed an `AccountId` where a
`CustomerId` was expected. Both compile; the bug hides until runtime. Concepts fix that by giving each
domain value its own type.

Say a `Person` is identified by a social security number. Instead of a bare `string`:

```csharp
public record Person(string SocialSecurityNumber);
```

wrap the value in a concept:

```csharp
public record SocialSecurityNumber(string Value) : ConceptAs<string>(Value)
{
    public static implicit operator SocialSecurityNumber(string value) => new(value);
}

public record Person(SocialSecurityNumber SocialSecurityNumber);
```

Now your APIs document themselves and the compiler enforces them:

```csharp
public interface IPersons
{
    Person GetBy(SocialSecurityNumber socialSecurityNumber);   // can't pass a CustomerId here
}
```

The implicit operator means you still write `SocialSecurityNumber id = "12345678901";` when it's
convenient — you get type safety *and* ergonomics. And because Fundamentals ships the serializers and
type converters for `ConceptAs<>`, a concept travels across JSON and HTTP as its plain underlying value,
not as a `{ "Value": … }` wrapper. (If you're on the Cratis Application Model, that wiring is automatic.)

That's the whole idea: **never use raw primitives for domain values.** Read the full pattern — sentinels,
nullability, serialization, and type converters — in [Concepts](./concepts.md).

## What else is in the box

| Topic | What it gives you |
| ------- | ----------- |
| [Concepts](./concepts.md) | The `ConceptAs<>` pattern, serialization, and type converters for strongly-typed values. |
| [Dependency Injection](./dependency_injection.md) | Convention-based service registration — artifacts are discovered, not hand-wired. |
| [Types](./types.md) | The type-discovery mechanism that finds your implementations at runtime. |
| [Serialization](./serialization/index.md) | JSON serialization, derived-type handling, and date/time converters. |
| [Metrics](./metrics/index.md) | A source-generated metrics system for tracking application behavior. |

Concepts and type discovery are exactly what let [Chronicle](/chronicle/) and [Arc](/arc/) find your
events, commands, and read models by convention — see [Why Cratis](/why-cratis/) for the bigger picture.
