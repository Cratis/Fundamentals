---
description: Configure the default JsonSerializerOptions in .NET with support for concepts, DateOnly, TimeOnly, polymorphism and type discriminators.
---
# JSON Serialization

## JsonSerializerOptions

Pass `Cratis.Json.Globals.JsonSerializerOptions` explicitly to `System.Text.Json.JsonSerializer` to use the Fundamentals JSON configuration.
It includes converters for [concepts](../concepts.md), `DateOnly`, `TimeOnly`, enums, derived types, and additional types.
Accessing this property does not register the options in dependency injection or configure ASP.NET JSON pipelines.

```csharp
using System;
using System.Text.Json;

var options = Cratis.Json.Globals.JsonSerializerOptions;
var json = JsonSerializer.Serialize(new Person("Ada"), options);
Console.WriteLine(json); // {"firstName":"Ada"}
var person = JsonSerializer.Deserialize<Person>(json, options);
Console.WriteLine(person!.FirstName); // Ada

public record Person(string FirstName);
```

The options use acronym-friendly **camelCase** property names and set `DefaultIgnoreCondition` to `WhenWritingNull`.
Naming applies to both serialization and deserialization, but does not enable case-insensitive input: the options leave `PropertyNameCaseInsensitive` at its default, `false`.
Custom converters can have their own naming and null-handling behavior.

## Enums under NativeAOT

The type-discovery generator emits enum converter factories for enums declared in each participating assembly. `Cratis.Json.EnumConverterFactory` uses these factories under NativeAOT, avoiding runtime generic converter construction. Keep using `new EnumConverterFactory()` in your serializer options and use a source-generated `JsonSerializerContext` for the types you serialize, as required by System.Text.Json under NativeAOT.

Enums must be accessible to generated code, including every containing type. Private, protected-only, private-protected, file-local, and generic containing types cannot be used. For enums in an assembly without generated discovery output, explicitly register `new EnumConverter<MyEnum>()` in your serializer options before the factory. Dynamic-code runtimes retain the existing reflection path and behavior.

## Polymorphism and Type Discriminators

When working with polymorphism and serialization, serializers don't generally know the concrete type that should be used for serializing from and
deserializing to. They need something that helps identify the actual type to perform the operation.

Different serializers deal with this in different ways. Therefore, we have implemented an approach that can be adopted into different serializers,
without having different approaches and serializer-specific metadata.

When serializing to a target format from a type, serializers tend to include type information to be able to deserialize it to the same type.
The problem with this approach is that when persisting this to a database, your code and database have to match and you can't rename or move
your code to another namespace. Some serializers offer a way to define a discriminator which could be a string or a unique identifier
that does not couple you to the name.

In the Cratis Fundamentals, we have taken the latter approach, but made it a consistent approach independent of the serializers.

Let's say you have an interface like below:

```csharp
public interface IAccount
{
    AccountId Id { get; }
    AccountName Name { get; }
    AccountType Type { get; }
}
```

To create concrete implementations of this, all you need is to add a `[DerivedType]` attribute in front of it.
So for our sample, let's say we add a `DebitAccount` and a `CreditAccount`:

```csharp
using Cratis.Serialization;

[DerivedType("2c025801-2223-402c-a42a-893845bb1077")]
public record DebitAccount(AccountId Id, AccountName Name, AccountType Type) : IAccount;

[DerivedType("b67b4e5b-d192-404b-ba6f-9647202bd20e")]
public record CreditAccount(AccountId Id, AccountName Name, AccountType Type) : IAccount;
```

The `[DerivedType]` attribute accepts a unique string identifier; a `Guid` string is one possible choice.
These account excerpts assume application-defined `AccountId`, `AccountName`, and `AccountType` types.

### JSON

With the Fundamentals options, serialize through the declared interface (such as `JsonSerializer.Serialize<IAccount>(account, options)`) to use the derived-type converter.
The converter adds `_derivedTypeId` for an attributed concrete type, so the consumer can recognize it.
Deserialize through the same interface using `JsonSerializer.Deserialize<IAccount>(json, options)`; the identifier selects a registered derivative of that interface.
If `_derivedTypeId` is missing, the converter returns the default value (`null` for an interface).
Serializing a concrete type directly does not by itself select this converter merely because it has `[DerivedType]`.

### Client

In the `@cratis/fundamentals` package you'll find something called `JsonSerializer`.
The serializer supports proper type deserialization based on metadata.

```typescript
export class AccountHolderWithAccounts {

    @field(String)
    firstName!: string;

    @field(String)
    lastName!: string;

    @field(String)
    socialSecurityNumber!: string;

    @field(Address)
    address!: Address;

    @field(Object, true, [
        CreditAccount,
        DebitAccount
    ])
    accounts!: IAccount[];
}
```

With an interface definition as follows:

```typescript
export interface IAccount {
    id: string;
    name: string;
    type: AccountType;
}
```

For derivatives, you'd typically have the following with the usage of the `@derivedType()` decorator.

```typescript
@derivedType('2c025801-2223-402c-a42a-893845bb1077')
export class DebitAccount {

    @field(String)
    id!: string;

    @field(String)
    name!: string;

    @field(Number)
    type!: AccountType;
}

@derivedType('b67b4e5b-d192-404b-ba6f-9647202bd20e')
export class CreditAccount {

    @field(String)
    id!: string;

    @field(String)
    name!: string;

    @field(Number)
    type!: AccountType;
}
```
