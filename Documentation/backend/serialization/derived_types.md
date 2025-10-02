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

- **Target Type**: The interface or base class that defines the contract (e.g., `IPaymentMethod`)
- **Derived Type**: The concrete implementation decorated with `[DerivedType]` (e.g., `CreditCard`, `PayPal`)

### Unique Identifiers

Each derived type must have a globally unique identifier (GUID) that remains constant across versions. This identifier is used during serialization to determine which concrete type to instantiate during deserialization.

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
[DerivedType("550e8400-e29b-41d4-a716-446655440001")]
public class CreditCard : IPaymentMethod
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; }
    public string ExpiryDate { get; set; }
}

[DerivedType("550e8400-e29b-41d4-a716-446655440002")]
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
    "_derivedTypeId": "550e8400-e29b-41d4-a716-446655440001"
  }
}
```

## Advanced Scenarios

### Multiple Interfaces

If a derived type implements multiple interfaces, you must specify which interface it represents:

```csharp
public interface IPaymentMethod { }
public interface IRefundable { }

[DerivedType("550e8400-e29b-41d4-a716-446655440001", typeof(IPaymentMethod))]
public class CreditCard : IPaymentMethod, IRefundable
{
    // Implementation
}
```

### Target Type Resolution

The system automatically resolves target types based on implemented interfaces. If a class implements only one interface decorated with derived types, that interface becomes the target type automatically.

### Validation Rules

The system enforces several validation rules:

1. **Unique Identifiers**: Each derived type must have a unique GUID identifier
2. **Single Target Type**: A derived type can only represent one target interface
3. **Interface Implementation**: Derived types must implement their declared target interface

## Error Handling

The system provides specific exceptions for common issues:

- **`MissingTargetTypeForDerivedType`**: Thrown when a derived type doesn't implement any recognizable interface
- **`AmbiguousTargetTypeForDerivedType`**: Thrown when a derived type implements multiple interfaces without specifying the target
- **`MissingDerivedTypeForTargetType`**: Thrown when trying to deserialize an unknown derived type identifier
- **`AmbiguousDerivedTypeIdentifiers`**: Thrown when multiple types use the same identifier

## Best Practices

### 1. Stable Identifiers

Always use stable GUIDs that won't change across versions:

```csharp
// ✅ Good - Use a stable GUID
[DerivedType("550e8400-e29b-41d4-a716-446655440001")]
public class CreditCard : IPaymentMethod { }

// ❌ Bad - Don't generate GUIDs dynamically
[DerivedType(Guid.NewGuid().ToString())]
public class PayPal : IPaymentMethod { }
```

### 2. Meaningful Identifiers

Consider using meaningful GUID generation for better maintainability:

```csharp
// Generate from namespace + type name for consistency
[DerivedType("550e8400-e29b-41d4-a716-446655440001")] // MyApp.Payments.CreditCard
public class CreditCard : IPaymentMethod { }
```

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

[DerivedType("events-user-created-v1")]
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

[DerivedType("commands-create-user-v1")]
public class CreateUser : ICommand
{
    public string Email { get; set; }
    public string Name { get; set; }
}
```
