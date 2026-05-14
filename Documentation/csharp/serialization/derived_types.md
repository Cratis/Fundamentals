# DerivedTypes Serialization (Backend)

The DerivedTypes serialization system in the .NET backend provides a powerful mechanism for polymorphic serialization, allowing you to serialize and deserialize objects where the exact type is determined at runtime. This is particularly useful when working with interfaces or base classes that can have multiple implementations.

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

```csharp
services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new DerivedTypeJsonConverterFactory());
});
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

When serialized, derived types include a special `_derivedTypeId` property:

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

Derived type IDs must be **unique per interface** to enable proper polymorphic deserialization. The identifier is an arbitrary string — use whatever format makes sense for your domain:

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

- The `DerivedTypes` registry is built once at startup by scanning all assemblies
- Use the static `DerivedTypes.Instance` for better performance in hot paths
- Consider the impact of reflection during type scanning in large applications

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
