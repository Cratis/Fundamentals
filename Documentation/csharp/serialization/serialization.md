# JSON Serialization

## JsonSerializerOptions

When using the `.AddControllersFromProjectReferencedAssemblies()` method, it sets up the default JSON options for serialization for API controllers.
This configuration includes support for [concepts](../concepts.md), `DateOnly`, `TimeOnly`, and additional converters.
The options are set up as the default options for the ASP.NET pipelines.

If you want to have access to the default `JsonSerializerOptions` in your own code, you can simply take a dependency in your constructor to it:

```csharp
using System.Text.Json;

public class MyService
{
    public MyService(JsonSerializerOptions options)
    {
        // use the options
    }
}
```

Default behavior for the JSON serializer options is to use **camelCase** instead of **PascalCase** and automatically translate between
the two when serializing and deserializing.

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

The `[DerivedType]` attribute requires a unique identifier in the form of a string representation of a `Guid`.

### JSON

The JSON serializer will add a `_derivedTypeId` to the payload referring to the type discriminator of the type.
With this, the consumer can recognize the type.

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
