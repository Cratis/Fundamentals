---
title: DerivedTypes serialization for .NET
description: Configure polymorphic .NET JSON serialization with DerivedTypeAttribute and a type registry so interface properties read back as concrete types.
---
<a id="derivedtypes-serialization-backend"></a>

Cratis Fundamentals uses `DerivedTypeAttribute` and a type registry to serialize .NET objects through an interface or base class and read them back as concrete types. Use this when an `Order` declares its payment method as `IPaymentMethod`, but the value is a `CreditCard` or `PayPal`.

The JSON carries an identifier for the concrete type. To complete the round trip, the serializer must use options with the derived-type converter, and its registry must contain your payment types.

## Overview

Mark each concrete class with `DerivedTypeAttribute`. The `DerivedTypes` registry finds these classes through type discovery and connects each identifier to the types it can represent. The registry implements `IDerivedTypes`, which the converter factory uses to create a `DerivedTypeJsonConverter<T>` for a declared target type.

## Key concepts

### Target types and derived types

A target type is the declared interface or base class, such as `IPaymentMethod`. A derived type is the concrete class marked with `[DerivedType]`, such as `CreditCard`.

The registry chooses one primary target: the class's single non-system interface, or the explicit `targetType` supplied on the attribute. It also registers non-system base classes, so a property declared as a base class can use the same identifier. Here, non-system means the type's namespace does not start with `System`.

### Unique identifiers

Choose a stable, descriptive string such as `"credit-card"` or `"paypal"`. The converter writes it into JSON and uses it to find the concrete type when reading. Changing the identifier means existing payloads still refer to the old value. Identifiers must be unique across all attributed types in the registry; see [Uniqueness Requirements](#uniqueness-requirements).

## Usage

These examples use `System.Text.Json` and types from `Cratis.Serialization`. Define the payment contract and implementations in your application, then configure the options used by the code that writes and reads them. Later sections reuse these names in separate illustrative snippets.

### 1. Define your target interface

```csharp
public interface IPaymentMethod
{
    decimal Amount { get; }
}
```

### 2. Create derived types

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

### 3. Configure JSON serialization

Use `Cratis.Json.Globals.JsonSerializerOptions` if you want the global options: they already include the derived-type converter factory. You do not need to add it again.

For separately configured options, the factory needs an `IDerivedTypes` registry containing your `[DerivedType]` classes. The following fragments configure `JsonSerializerOptions` on an existing service collection. Choose one registration, and pass the configured options to the serializer; configuring the service collection alone does not change every serialization call.

Also set `PropertyNamingPolicy` to `JsonNamingPolicy.CamelCase`, or enable `PropertyNameCaseInsensitive`, for the payment examples to read back correctly. The derived-type converter writes property names in camel case, then delegates reading to the concrete type's `System.Text.Json` contract. Adding the converter alone does not configure that contract's naming.

To use the shared registry, pass `DerivedTypes.Instance`:

```csharp
using Cratis.Serialization;

services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new DerivedTypeJsonConverterFactory(DerivedTypes.Instance));
});
```

If your application uses [convention-based dependency injection](../dependency_injection.md#addbindingsbyconvention), `AddBindingsByConvention()` binds `IDerivedTypes` to `DerivedTypes`. That registry needs an `ITypes` registration, for example from `AddTypeDiscovery()`. Resolve it from the container so the converter uses the same type discovery as your services:

```csharp
using Cratis.Serialization;

services.AddOptions<JsonSerializerOptions>()
    .Configure<IDerivedTypes>((options, derivedTypes) =>
        options.Converters.Add(new DerivedTypeJsonConverterFactory(derivedTypes)));
```

### 4. Use in your models

Declare the payment properties with `IPaymentMethod` so the converter can select a concrete type. Serialize and deserialize `Order` using the configured options. For a top-level payment value, use `IPaymentMethod` as the serialization type too; serializing a concrete class directly does not select this converter just because the class has `[DerivedType]`.

```csharp
public class Order
{
    public string OrderId { get; set; }
    public IPaymentMethod PaymentMethod { get; set; }
    public IEnumerable<IPaymentMethod> AlternativePayments { get; set; }
}
```

## Serialization format

The converter writes `_derivedTypeId`, the JSON property that identifies the concrete type. In this `Order` payload, `"credit-card"` identifies the `CreditCard` held by `PaymentMethod`. On reading, the registry looks up that identifier together with the declared `IPaymentMethod` target. The camel-case `orderId` and `paymentMethod` names match the global options:

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

## Uniqueness requirements

Every attributed type discovered by the registry must have a different identifier, even when the types implement different interfaces. The `DerivedTypes` constructor rejects duplicates across its entire `ITypes` set; it does not check separate applications. Use strings that make sense in your domain:

```csharp
[DerivedType("credit-card")]
public class CreditCard : IPaymentMethod { }

[DerivedType("paypal")]
public class PayPal : IPaymentMethod { }
```

With these names, you can recognize the payment type in a payload without looking up an opaque identifier.

## Advanced scenarios

### Multiple interfaces

If a derived type implements multiple non-system interfaces, specify the target explicitly. Here, `CreditCard` represents `IPaymentMethod`; implementing `IRefundable` does not make it another primary target:

```csharp
public interface IPaymentMethod { }
public interface IRefundable { }

[DerivedType("credit-card", typeof(IPaymentMethod))]
public class CreditCard : IPaymentMethod, IRefundable
{
    // Implementation
}
```

### Target type resolution

Without an explicit `targetType`, the registry requires exactly one non-system interface and uses it as the primary target. Supplying `targetType` selects that target instead, but the class must still implement at least one non-system interface and be assignable to the supplied type.

All non-system base classes in the inheritance chain are additional targets. They let the registry resolve the concrete class for base-class properties, but do not remove the interface requirement.

### Validation rules

Registry construction checks identifier uniqueness, requires a non-system interface, and rejects ambiguous interface selection when no target is supplied. For an explicit target, it checks that the concrete type is assignable to it. Resolve these registration errors before using the converter.

## Error handling

Use the exception to locate the failed registration or lookup:

- `MissingTargetTypeForDerivedType`: the class has no non-system interface, or a requested derived type has no registered primary target.
- `AmbiguousTargetTypeForDerivedType`: the class has multiple non-system interfaces and no explicit target.
- `TargetTypeMismatchForDerivedType`: the class is not assignable to the explicit target.
- `MissingDerivedTypeForTargetType`: the registry has no match for the requested target and identifier.
- `AmbiguousDerivedTypeIdentifiers`: multiple discovered types use the same identifier.

A missing `_derivedTypeId` behaves differently from an unknown value: the converter returns the default value when the property is absent. If a payment method comes back `null`, inspect the payload for that property first. If lookup throws, compare the identifier and declared target with the registry, then inspect [what type discovery found](../types.md#knowing-what-was-discovered) before changing an identifier.

## Best practices

### 1. Stable identifiers

Keep identifiers stable across versions so stored payloads continue to resolve. Attribute arguments must also be compile-time constants; the dynamic expression marked as bad below is not valid C#:

```csharp
// ✅ Good - Use a stable descriptive string
[DerivedType("credit-card")]
public class CreditCard : IPaymentMethod { }

// ❌ Bad - Don't generate identifiers dynamically
[DerivedType(Guid.NewGuid().ToString())]
public class PayPal : IPaymentMethod { }
```

### 2. Centralize type identifiers (magic strings)

If several parts of the application refer to these identifiers, put them in shared constants:

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

The constants give each identifier one place to be defined. They do not enforce uniqueness or synchronize another client; the registry still checks the discovered types for duplicates.

### 3. Interface design

Treat the target interface as a serialized contract. Consider how existing payloads will be read before adding required members or changing their types:

```csharp
public interface IPaymentMethod
{
    decimal Amount { get; }
    // Add new properties with default implementations or make them optional
}
```

### 4. Testing

Check a round trip with the options your application actually uses. This test fragment assumes `options` is configured with a registry containing the payment types and that your test project supplies the shown assertion methods. Checking the concrete type and `CardNumber` catches both a missing identifier and a lost property:

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

## Integration with dependency injection

`DerivedTypes` carries the `[Singleton]` attribute for convention-based registration:

```csharp
[Singleton]
public class DerivedTypes : IDerivedTypes
```

After registering type discovery and convention-based bindings, inject `IDerivedTypes` to query the registry. `IsDerivedType` checks whether the concrete type has a registered primary target:

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

## Performance considerations

Reuse the registry: constructing `DerivedTypes` builds lookup maps from its supplied type-discovery result. `DerivedTypes.Instance` uses global type discovery, while a dependency-injected registry uses the `ITypes` supplied by the container. Choose the registry that contains your application's types; switching to the static instance is not a serialization-speed setting.

## Common patterns

### Event sourcing

An event interface can use the same mechanism. In this illustrative contract, `"user-created"` identifies the concrete event when it is serialized through `IEvent`:

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

### Command/query pattern

The same registration works for a command interface. Here, `"create-user"` identifies `CreateUser` when the declared type is `ICommand`:

```csharp
public interface ICommand { }

[DerivedType("create-user")]
public class CreateUser : ICommand
{
    public string Email { get; set; }
    public string Name { get; set; }
}
```
