---
title: C# (.NET)
description: Use Cratis.Fundamentals on its own for strongly-typed concepts, serialization, dependency injection, type discovery, and explicit optional or failure outcomes.
---

A method that accepts several strings cannot tell you that you passed a customer identifier where a person's social security number belongs. `Cratis.Fundamentals` gives those values distinct types, along with the serialization and type-conversion support to use them across your application.

You can use the [NuGet package](https://www.nuget.org/packages/Cratis.Fundamentals/) in an ordinary .NET project. Arc and Chronicle use it too, but neither is required for the examples on this page.

## Define your first concept

With a .NET 10 SDK, create a console app and add the package:

```bash
dotnet new console --framework net10.0 --name ConceptsDemo
cd ConceptsDemo
dotnet add package Cratis.Fundamentals --version 7.32.0
```

Replace `Program.cs` with this complete program:

```csharp
using System;
using Cratis.Concepts;

public static class ConceptExample
{
    public static void Main()
    {
        Person person = new(new SocialSecurityNumber("example-only"));
        Console.WriteLine(person.SocialSecurityNumber.Value);
    }

    public record SocialSecurityNumber(string Value) : ConceptAs<string>(Value)
    {
        public static implicit operator SocialSecurityNumber(string value) => new(value);
    }

    public record Person(SocialSecurityNumber SocialSecurityNumber);
}
```

`Person` now accepts a `SocialSecurityNumber`, so you cannot accidentally pass a different concept with a string underneath it. The implicit conversion is a convenience when you intentionally start from a string. Wrapping the string does not validate its contents; `example-only` is synthetic text, not a real identifier.

Run the example:

```bash
dotnet run
```

Expected output:

```text
example-only
```

That first concept needs no service registration. For sentinels, validation considerations, JSON serialization, and type converters, continue with [Concepts](./concepts.md).

## Make absence and rejection explicit

A typed identifier tells you what a value means. A return type can also tell you whether a value exists or why an operation rejected it. The `Cratis.Monads` namespace provides `Option<T>` for presence or absence, `Result<TResult, TError>` for a value or typed error, and `Catch<TResult>` for a value or captured exception.

[Start the monads chapter](./monads/index.md) to follow a text setting through lookup, validation, and a throwing parser. Each lesson runs in a standalone console app with the same package; there is no host, database, or Cratis service to configure.

## What else is in the box

| Topic | What it gives you |
| --- | --- |
| [Concepts](./concepts.md) | Distinct domain value types with `ConceptAs<T>`, plus serialization and type conversion. |
| [Monads](./monads/index.md) | Explicit absence, typed operation errors, and captured exceptions with Option, Result, and Catch. |
| [Dependency injection](./dependency_injection.md) | Convention-based service registration. |
| [Types](./types.md) | Runtime discovery of types and implementations. |
| [Serialization](./serialization/index.md) | JSON serialization, derived-type handling, and date/time converters. |
| [Metrics](./metrics/index.md) | A source-generated metrics system for tracking application behavior. |

These building blocks also support artifact discovery in [Chronicle](/chronicle/) and [Arc](/arc/). [Why developers choose Cratis](/why-cratis/) explains how they fit into the wider stack.
