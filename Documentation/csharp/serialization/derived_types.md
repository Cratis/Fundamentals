---
description: Serialize and deserialize polymorphic .NET objects by runtime type with DerivedTypeAttribute and the DerivedTypes registry.
---
# DerivedTypes Serialization (Backend)

An Order can declare its payment method as IPaymentMethod while holding a CreditCard or PayPal value. To read that value back as the right concrete type, the JSON needs an identifier and the serializer needs a registry that can resolve it. This guide connects the payment types, registry and converter used for that round trip.

## Overview

The DerivedTypes system consists of several key components:

- **`DerivedTypeAttribute`**: Marks classes as derived types with unique identifiers
- **`DerivedTypes`**: Registry that tracks relationships between derived types and their target types
- **`DerivedTypeJsonConverter<T>`**: Custom JSON converter for handling polymorphic serialization
- **`IDerivedTypes`**: Interface defining the contract for the derived types system

## Key Concepts

### Target Types and Derived Types

- **Primary Target Type**: The type used as the canonical registration key for a derived type.
    By default this is the inferred non-system interface (or the explicit `targetType` if provided).
- **Additional Target Types**: Non-system base classes are registered automatically so lookup can resolve
    by base class without requiring explicit `targetType`.
- **Derived Type**: The concrete implementation decorated with `[DerivedType]` (e.g., `CreditCard`, `PayPal`)

### Unique Identifiers

Each derived type must have a unique string identifier that remains constant across versions. This identifier is used during serialization to determine which concrete type to instantiate during deserialization.

The recommended approach is to use **descriptive strings** (e.g., `"credit-card"`, `"paypal"`) for readability. See the Uniqueness Requirements section below for details and alternatives.

## Usage

### 1. Define Your Target Interface

```csharp
public interface IPaymentMethod
{
    decimal Amount { get; }
}
```

### 2. Create Derived Types

```csharp
[DerivedType("credit-card")]
public class CreditCard : IPaymentMethod
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; }
    public string ExpiryDate { get; set; }
}

[DerivedType("paypal")]
public class PayPal : IPaymentMethod
{
    public decimal Amount { get; set; }
    public string Email { get; set; }
}
```

### 3. Configure JSON Serialization

If you pass `Cratis.Json.Globals.JsonSerializerOptions` to the serializer, those options already include the derived-type converter factory. The registration fragments below are for separately configured options on an existing service collection; they are alternatives, not complete standalone programs. Choose the registry that contains your application's discovered types. If a payment type is missing, inspect [what type discovery found](../types.md#knowing-what-was-discovered) before changing its identifier.

`DerivedTypeJsonConverterFactory` has no parameterless constructor: it needs the `IDerivedTypes` registry that knows your `[DerivedType]` types. Pass the shared `DerivedTypes.Instance` for explicit registration:

```csharp
using Cratis.Serialization;

services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new DerivedTypeJsonConverterFactory(DerivedTypes.Instance));
});
```

If you register services by convention (`AddBindingsByConvention()`), `IDerivedTypes` is bound to `DerivedTypes`, which needs an `ITypes` registration (for example from `AddTypeDiscovery()`). In that case resolve the registry from the container instead of using the static instance:

```csharp
using Cratis.Serialization;

services.AddOptions<JsonSerializerOptions>()
    .Configure<IDerivedTypes>((options, derivedTypes) =>
        options.Converters.Add(new DerivedTypeJsonConverterFactory(derivedTypes)));
```

### 4. Use in Your Models

```csharp
public class Order
{
    public string OrderId { get; set; }
    public IPaymentMethod PaymentMethod { get; set; }
    public IEnumerable<IPaymentMethod> AlternativePayments { get; set; }
}
```

## Serialization Format

When serialized, derived types include a special `_derivedTypeId` property. In the Order payload below, `credit-card` identifies the CreditCard value held by PaymentMethod; the registry uses that identifier together with the declared target type to resolve the concrete type when reading:

```json
{
  "orderId": "12345",
  "paymentMethod": {
    "amount": 99.99,
    "cardNumber": "****-****-****-1234",
    "expiryDate": "12/25",
    "_derivedTypeId": "credit-card"
  }
}
```

## Uniqueness Requirements

Derived type IDs must be **unique across all attributed types discovered by the registry** (the `ITypes` universe it is built from), not only per interface, to enable proper polymorphic deserialization. The `DerivedTypes` constructor rejects duplicate identifiers across that whole set. Uniqueness across separate applications is not checked. The identifier is an arbitrary string — use whatever format makes sense for your domain:

```csharp
[DerivedType("credit-card")]
public class CreditCard : IPaymentMethod { }

[DerivedType("paypal")]
public class PayPal : IPaymentMethod { }
```

Descriptive strings are strongly preferred — they make the serialized JSON readable and debugging straightforward.

## Advanced Scenarios

### Multiple Interfaces

If a derived type implements multiple interfaces, you must specify which interface it represents:

```csharp
public interface IPaymentMethod { }
public interface IRefundable { }

[DerivedType("credit-card", typeof(IPaymentMethod))]
public class CreditCard : IPaymentMethod, IRefundable
{
    // Implementation
}
```

### Target Type Resolution

The system resolves a primary target type automatically from implemented non-system interfaces.
If a class implements exactly one non-system interface, that interface becomes the primary target.
If `targetType` is provided on `[DerivedType(..., targetType)]`, that value is used as the primary target.

In addition to the primary target, the system now also registers the derived type for all non-system base
classes in its inheritance chain. This means polymorphic deserialization can work when the property is typed
as either an interface or a base class.

### Validation Rules

The system enforces several validation rules:

1. **Unique Identifiers**: Each derived type must have a unique string identifier
2. **Single Target Type**: A derived type can only represent one target interface
3. **Interface Implementation**: Derived types must implement their declared target interface
4. **Base Class Compatibility**: Derived types are also registered for non-system base classes automatically

## Error Handling

The system provides specific exceptions for common issues:

- **`MissingTargetTypeForDerivedType`**: Thrown when a derived type doesn't implement any recognizable interface
- **`AmbiguousTargetTypeForDerivedType`**: Thrown when a derived type implements multiple interfaces without specifying the target
- **`MissingDerivedTypeForTargetType`**: Thrown when trying to deserialize an unknown derived type identifier
- **`AmbiguousDerivedTypeIdentifiers`**: Thrown when multiple types use the same identifier

A missing `_derivedTypeId` is different from an unknown identifier: the converter returns the default value when the property is absent, but asks the registry to resolve an identifier when it is present. If a payment method comes back null, first inspect the payload for the discriminator; if lookup fails, compare the identifier and declared target with the types in the registry.

## Best Practices

### 1. Stable Identifiers

Always use stable identifiers that won't change across versions:

```csharp
// ✅ Good - Use a stable descriptive string
[DerivedType("credit-card")]
public class CreditCard : IPaymentMethod { }

// ❌ Bad - Don't generate identifiers dynamically
[DerivedType(Guid.NewGuid().ToString())]
public class PayPal : IPaymentMethod { }
```

### 2. Centralize Type Identifiers (Magic Strings)

Store derived type identifiers in a shared constants file to ensure consistency across your codebase:

```csharp
// Constants/DerivedTypeIds.cs
public static class DerivedTypeIds
{
    public const string CreditCard = "credit-card";
    public const string PayPal = "paypal";
    public const string BankTransfer = "bank-transfer";
}

// Usage
[DerivedType(DerivedTypeIds.CreditCard)]
public class CreditCard : IPaymentMethod { }

[DerivedType(DerivedTypeIds.PayPal)]
public class PayPal : IPaymentMethod { }
```

> **Why centralize?** These "magic strings" are critical for serialization consistency. Centralizing them prevents duplicates across your codebase, makes it easy to maintain and update, and ensures frontend and backend remain synchronized.

### 3. Interface Design

Design interfaces to be stable and avoid breaking changes:

```csharp
public interface IPaymentMethod
{
    decimal Amount { get; }
    // Add new properties with default implementations or make them optional
}
```

### 4. Testing

Always test serialization round-trips:

```csharp
[Fact]
public void should_serialize_and_deserialize_credit_card()
{
    var original = new CreditCard { Amount = 99.99m, CardNumber = "1234" };
    var json = JsonSerializer.Serialize<IPaymentMethod>(original, options);
    var deserialized = JsonSerializer.Deserialize<IPaymentMethod>(json, options);

    deserialized.ShouldBeOfExactType<CreditCard>();
    ((CreditCard)deserialized).CardNumber.ShouldEqual("1234");
}
```

## Integration with Dependency Injection

The `DerivedTypes` class is registered as a singleton in the DI container:

```csharp
[Singleton]
public class DerivedTypes : IDerivedTypes
```

You can inject `IDerivedTypes` into your services to programmatically work with the type registry:

```csharp
public class PaymentService
{
    private readonly IDerivedTypes _derivedTypes;

    public PaymentService(IDerivedTypes derivedTypes)
    {
        _derivedTypes = derivedTypes;
    }

    public bool SupportsPaymentType(Type paymentType)
    {
        return _derivedTypes.IsDerivedType(paymentType);
    }
}
```

## Performance Considerations

- The `DerivedTypes` registry builds its lookup maps from the supplied type-discovery result.
- Reuse a registry rather than rebuilding its maps for each serialization call. The static `DerivedTypes.Instance` uses the global type-discovery instance; a dependency-injected registry uses its supplied type discovery.
- Choose the registry for the types your application discovers, rather than switching to the static instance on the assumption that it makes serialization faster.

## Common Patterns

### Event Sourcing

DerivedTypes are commonly used in event sourcing scenarios:

```csharp
public interface IEvent
{
    DateTime Timestamp { get; }
}

[DerivedType("user-created")]
public class UserCreated : IEvent
{
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
}
```

### Command/Query Pattern

Perfect for CQRS implementations:

```csharp
public interface ICommand { }

[DerivedType("create-user")]
public class CreateUser : ICommand
{
    public string Email { get; set; }
    public string Name { get; set; }
}
```
