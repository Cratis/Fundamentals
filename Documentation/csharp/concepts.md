---
uid: Fundamentals.Concepts
title: C# domain concepts with ConceptAs
description: ConceptAs gives domain values distinct C# types, so method signatures express their meaning while JSON converters keep the underlying value.
---

<a id="concepts"></a>

`ConceptAs<T>` in Cratis Fundamentals gives a single domain value its own C# type. Use it when a method needs a social security number, customer id, or other meaningful value rather than any `string` or `Guid`. The compiler can then check that callers pass the intended type.

A property name explains what a primitive value means, but that name is lost when a method accepts any value of the same primitive type. A concept carries the meaning into the signature. The examples below replace a person's string identifier with `SocialSecurityNumber`, then configure converters to keep its string representation in JSON.

`ConceptAs<T>` implements `IComparable` and delegates comparisons to its underlying value. The underlying type must also implement `IComparable`.

Keep each concept to one value. The supplied converters write and read only the wrapped `Value`; they do not preserve additional properties. Use a separate model containing several concepts when you need to represent a more complex structure.

## Enums

An enum already names a domain value and its allowed alternatives. Use it directly rather than wrapping it in `ConceptAs<T>`.

## Serialization

A concept gives you a distinct type in C#, but you may want its underlying value at a transport or storage boundary. For example, a JSON client can receive a social security number as a string rather than an object containing a `Value` property. The concept converters handle this translation in both directions. They require a constructor that accepts the single wrapped value to recreate the concept.

## Creating a Concept

Start with a `Person` whose identifier is a string:

```csharp
public record Person(string SocialSecurityNumber);
```

Define a record derived from `ConceptAs<string>` to give that identifier its own type:

```csharp
public record SocialSecurityNumber(string Value) : ConceptAs<string>(Value);
```

Use the concept in the model:

```csharp
public record Person(SocialSecurityNumber SocialSecurityNumber);
```

The same distinction now appears in a lookup method. `GetBy` requires a `SocialSecurityNumber`, not an arbitrary string:

```csharp
public interface IPersons
{
    Person GetBy(SocialSecurityNumber socialSecurityNumber);
}
```

The record's constructor takes the wrapped string, so converters can recreate it. This type distinction does not itself validate the format of a social security number; any domain validation remains your responsibility.

## Generating new values

When callers need a new id or code, put its generation rule on the concept. Implement `IGeneratable<TSelf>` to declare a static `New()` method that constrained generic code can call without reflection. Callers then ask the domain type for a new value without choosing the generation strategy themselves.

If the rule depends on the operation's captured time or tenant, implement `IGeneratableInContext<TSelf>` with `New(GenerationContext context)`. The caller supplies the context, so values generated for one operation can share its captured time. If a type implements both interfaces, callers use the context form.

Choose a `GenerateValue` method for the value you need:

| Value needed | Method |
| --- | --- |
| Random identifier | `Uuid()` |
| Time-ordered identifier | `UuidV7()`, `Ulid()`, or `TypeId(prefix)`; each also accepts a `GenerationContext` |
| Random string | `NanoId()` or `Bytes(count)` |
| Readable code with optional check digits | `Pattern(pattern, checksum)` |
| Number from an inclusive Int64 range | `Integer(minimum, maximum)` |

Date-bearing patterns require `Pattern(pattern, context, checksum)` and use the captured UTC date. Check supplied values explicitly with `HasValidChecksum(value, checksum)`.

To build concepts that generate their own ids and codes, follow [Generating values](generating-values/index.md). That section also helps you choose between a public id, a readable code, and a secret, and test the rule with known values or a pinned clock.

## Implicit Operators

The `ConceptAs<T>` base record provides an implicit conversion from the concept to its underlying value. To allow conversion in the other direction, define an operator on your concept. This is useful when callers often receive the value as a primitive:

```csharp
public record SocialSecurityNumber(string Value) : ConceptAs<string>(Value)
{
    public static implicit operator SocialSecurityNumber(string value) => new(value);
}
```

With this operator, a string can initialize a `SocialSecurityNumber`, and the base operator can unwrap it again:

```csharp
string socialSecurityNumber = "12345678901";
SocialSecurityNumber formalized = socialSecurityNumber;
string unwrapped = formalized;
```

## Nullability

`ConceptAs<T>` rejects a null wrapped value. Represent absence with a nullable concept reference instead, so the model states whether the value is optional.

This declaration expects a social security number:

```csharp
public record Person(SocialSecurityNumber SocialSecurityNumber);
```

If a person may have no social security number, make the property nullable:

```csharp
public record Person(SocialSecurityNumber? SocialSecurityNumber);
```

The nullable annotation makes absence visible to callers. If the concept has an implicit conversion from its primitive type, that operator can return a null concept for a null input:

```csharp
public record SocialSecurityNumber(string Value) : ConceptAs<string>(Value)
{
    public static implicit operator SocialSecurityNumber?(string value) => value == default ? default : new(value);
}
```

## System.Text.Json

The `Cratis.Json` namespace contains converters for writing concepts as their underlying values and reading them back.

Without a converter, `JsonSerializer` writes the concept as an object. This example uses the implicit conversion defined above:

```csharp
using System.Text.Json;

var person = new Person("12345678901"); // Implicit operator converting it to SocialSecurityNumber
var serialized = JsonSerializer.Serialize(person);
```

The output contains a nested `Value` property:

```json
{
    "SocialSecurityNumber": {
        "Value": "12345678901"
    }
}
```

Add `ConceptAsJsonConverterFactory` to the options passed to the serializer:

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new ConceptAsJsonConverterFactory()
    }
};

var person = new Person("12345678901"); // Implicit operator converting it to SocialSecurityNumber
var serialized = JsonSerializer.Serialize(person, options);
```

The same property is now a string in JSON:

```json
{
    "SocialSecurityNumber": "12345678901"
}
```

Use the same converter configuration when deserializing. It constructs `SocialSecurityNumber` from the incoming string:

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new ConceptAsJsonConverterFactory()
    }
};

var json = "{ \"SocialSecurityNumber\": \"12345678901\" }";
var person = JsonSerializer.Deserialize<Person>(json, options);
```

Cratis Arc configures the concept converters when setting up its host. In an application using Fundamentals directly, configure the JSON options used by your serializer or HTTP host.

## TypeConverters

.NET [type converters](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.typeconverter?view=net-6.0) serve consumers of `TypeDescriptor`, including ASP.NET controller parameter binding. They are separate from JSON converters. Fundamentals provides `ConceptAsTypeConverter<TConcept, TValue>`; register it for each concept type:

```csharp
using System.ComponentModel;
using Cratis.Concepts;

TypeDescriptor.AddAttributes(typeof(SocialSecurityNumber), new TypeConverterAttribute(typeof(ConceptAsTypeConverter<SocialSecurityNumber, string>)));
```

To avoid maintaining one registration per concept, use `RegisterTypeConvertersForConcepts` on an `ITypes` instance. It registers converters for the concepts found by [type discovery](./types.md):

```csharp
using Cratis.Concepts;
using Cratis.Types;

var types = new Types();
types.RegisterTypeConvertersForConcepts();
```

Cratis Arc registers these type converters during host setup. When using Fundamentals directly, register them before a consumer needs to convert concept values. If a concept is missing from discovery, follow [Types](./types.md) to inspect which assemblies contributed.
