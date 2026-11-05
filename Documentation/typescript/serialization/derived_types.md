---
title: TypeScript JsonSerializer for polymorphic models
description: Use derived type identifiers and field metadata to deserialize polymorphic JSON fields into TypeScript classes with JsonSerializer.
---
<a id="derivedtypes-serialization-frontend"></a>

`JsonSerializer` uses `_derivedTypeId` and field metadata to turn polymorphic JSON fields into their concrete TypeScript classes. Use this when a field can contain several implementations, such as a payment method that can be a credit card or PayPal payment. The identifier tells the reader which class to construct; `@field` tells it which values to populate.

This guide builds that payment model. For decorator syntax and compiler setup, use the [field decorator guide](field_decorator.md).

## Overview

A JSON object alone cannot tell JavaScript whether a payment is a `CreditCard` or `PayPal` instance. Add `@derivedType` to each implementation to give it an identifier, then declare the containing field with `@field`. At deserialization time, `JsonSerializer` matches the incoming identifier to a candidate constructor and reads that class's field metadata.

## Key Concepts

### Type Metadata System

Cratis Fundamentals stores the identifiers and constructors as runtime metadata. TypeScript's type annotations disappear during compilation, so declaring a property as `IPaymentMethod` is not enough to reconstruct it. The decorators supply the information that remains available when the JSON arrives. When exchanging derived types with .NET, use exactly the same identifier as the backend's `[DerivedType]` attribute.

### Runtime Resolution Behavior

At runtime, the deserializer resolves derived types from two candidate sources:

- `field.derivatives` provided by `@field(..., derivatives)`
- Runtime registrations from `DerivedType.getDerivedTypesFor(field.type)`

When a decorated class is evaluated, `@derivedType` registers it against every base class in its inheritance chain. Import the modules that define your subclasses before deserializing; an unloaded class has not registered itself.

Interfaces have no runtime constructor. For an interface-shaped field, provide the implementations explicitly in `field.derivatives`, as the payment example below does. Alternatively, pass a concrete `targetType` constructor to `@derivedType` and use that constructor as the field's runtime type.

If no candidate matches `_derivedTypeId`, the reader falls back to the field's declared constructor. It preserves the identifier on the resulting instance, but does not reconstruct an unknown implementation. Root calls such as `deserialize(BaseClass, json)` also use the supplied constructor; derived-type selection happens when reading declared fields.

### Decorator-Based Configuration

Use `@derivedType` on the class and `@field` on the values you need to read back. These serve different purposes: registering a class identifier does not declare its fields.

## Uniqueness Requirements

Give every candidate for a field a distinct identifier. The TypeScript reader selects the first candidate with a matching ID; it does not reject duplicate registrations. The .NET `DerivedTypes` registry rejects duplicate identifiers across its discovered types, so keep identifiers unique across the types you exchange with that backend.

Short, descriptive strings make payloads easier to inspect. These excerpts assume the `IPaymentMethod` interface defined in the usage section:

```typescript
@derivedType('credit-card')
export class CreditCard implements IPaymentMethod { }

@derivedType('paypal')
export class PayPal implements IPaymentMethod { }
```

GUID strings are another identifier format:

```typescript
@derivedType('550e8400-e29b-41d4-a716-446655440001')
export class CreditCard implements IPaymentMethod { }

@derivedType('550e8400-e29b-41d4-a716-446655440002')
export class PayPal implements IPaymentMethod { }
```

Whichever format you choose, keep the value stable and identical in both clients. Changing an identifier means previously written payloads no longer match that class.

## Usage

### 1. Define Your Interface

```typescript
export interface IPaymentMethod {
    amount: number;
}
```

### 2. Create Derived Types

Decorate both implementations with their payload identifiers. The `@field` declarations supply the runtime types for their data, including fields shared through the interface.

```typescript
import { derivedType } from '@cratis/fundamentals';
import { field } from '@cratis/fundamentals';

@derivedType('credit-card')
export class CreditCard implements IPaymentMethod {
    @field(Number)
    amount!: number;

    @field(String)
    cardNumber!: string;

    @field(String)
    expiryDate!: string;
}

@derivedType('paypal')
export class PayPal implements IPaymentMethod {
    @field(Number)
    amount!: number;

    @field(String)
    email!: string;
}
```

### 3. Configure Parent Classes with Derivatives

Use `Object` as the runtime field type and list the two implementations. The `true` argument on `alternativePayments` applies the same resolution to each array element.

```typescript
export class Order {
    @field(String)
    orderId!: string;

    @field(Object, false, [CreditCard, PayPal])
    paymentMethod!: IPaymentMethod;

    @field(Object, true, [CreditCard, PayPal])
    alternativePayments!: IPaymentMethod[];
}
```

With these declarations, a `paymentMethod` payload carrying `_derivedTypeId: "credit-card"` becomes a `CreditCard` instance. If you use a class hierarchy instead, pass the concrete base class to `@field`; its registered subclasses can then be found without an explicit list.

## Serialization and Deserialization

Call `JsonSerializer.serialize(order)` to write the order, or `JsonSerializer.deserialize(Order, json)` to read it. Serialization adds the identifier registered on each payment class. Deserialization uses the containing field's candidates to choose the payment constructor. The [JsonSerializer guide](./json_serializer.md) covers these calls and array inputs.

## Field Decorator Integration

Decorate each field that needs typed deserialization, including fields on the derived classes. Serialization writes runtime own properties even without `@field`, but reading a concrete class populates only declared fields. The [field decorator guide](field_decorator.md) explains nested types, collections, and metadata inspection.

## Serialization Format

The payment identifier travels with its data in `_derivedTypeId`, the discriminator property also used by the .NET derived-type converter:

```json
{
  "paymentMethod": {
    "amount": 99.99,
    "cardNumber": "****-1234",
    "expiryDate": "12/25",
    "_derivedTypeId": "credit-card"
  }
}
```

You do not need to declare `_derivedTypeId` as a field. `JsonSerializer` writes it from the class registration and preserves an incoming identifier when reading, even if the concrete class cannot be resolved.

## Advanced Scenarios

### Complex Nested Objects

Nesting does not change the field declarations. Here, reading each `Order` also reads its payment fields using the candidates already declared on `Order`.

```typescript
export class ShoppingCart {
    @field(String)
    cartId!: string;

    @field(Order, true)
    orders!: Order[];

    @field(Object, false, [CreditCard, PayPal])
    defaultPayment!: IPaymentMethod;
}
```

### Mixed Collections

A collection can list additional implementations. This excerpt assumes a `BankTransfer` class with its own `@derivedType` identifier and decorated fields.

```typescript
export class PaymentHistory {
    @field(Object, true, [CreditCard, PayPal, BankTransfer])
    transactions!: IPaymentMethod[];
}
```

### Well-Known Types

Use runtime constructors for other values on your model too. These field excerpts use the built-in `Date` and `Guid` converters:

```typescript
@field(Date)
createdAt!: Date;

@field(Guid)
id!: Guid;

@field(Boolean)
isActive!: boolean;
```

## Error Handling

### Common Issues

If a payment comes back as a plain object, check that its implementation module was imported and its `@derivedType` identifier matches the payload. Then check the containing field: an erased interface needs explicit candidates or a concrete runtime target registration.

If the class is correct but data is missing, inspect its `@field` declarations. An undecorated property can appear in outgoing JSON without having metadata to read it back.

### Debugging Tips

Inspect the identifier on the candidate and the fields on its containing model. Import `DerivedType` and `Fields` from `@cratis/fundamentals` for this excerpt.

```typescript
// Check if a type has derived type metadata
const derivedTypeId = DerivedType.get(CreditCard);
console.log(`CreditCard ID: ${derivedTypeId}`);

// Inspect field metadata
const fields = Fields.getFieldsForType(Order);
console.log('Order fields:', fields);
```

## Best Practices

### 1. Consistent Identifiers

Ensure derived type IDs match exactly between frontend and backend:

```typescript
// Frontend
@derivedType('credit-card')
export class CreditCard implements IPaymentMethod { }
```

```csharp
// Backend
[DerivedType("credit-card")]
public class CreditCard : IPaymentMethod { }
```

### 2. Centralize Type Identifiers

Store derived type IDs in a single location to avoid duplication and ensure consistency:

```typescript
// constants/derivedTypeIds.ts
export const DERIVED_TYPE_IDS = {
    CREDIT_CARD: 'credit-card',
    PAYPAL: 'paypal',
    BANK_TRANSFER: 'bank-transfer',
} as const;

// usage
@derivedType(DERIVED_TYPE_IDS.CREDIT_CARD)
export class CreditCard implements IPaymentMethod { }

@derivedType(DERIVED_TYPE_IDS.PAYPAL)
export class PayPal implements IPaymentMethod { }
```

Constants reduce repeated literals within the frontend. They do not synchronize a separately maintained backend, so compare the actual values on both sides when a payload fails to resolve.

### 3. Typed Field Metadata

Decorate every public field that requires typed deserialization. Runtime own properties also serialize without `@field`, but concrete-class deserialization only populates declared fields and cannot convert an undecorated value to its intended runtime type.

```typescript
export class CreditCard implements IPaymentMethod {
    @field(Number)
    amount!: number;

    @field(String)
    cardNumber!: string;

    // This runtime own property is serialized, but has no typed deserialization metadata
    transientNote: string = '';

    // Public serializable fields use @field
    @field(String)
    cardIssuer!: string;
}
```

### 4. Derivative Lists for Erased Types

For the interface-based payment model, keep the candidates on the field. The second declaration below has no candidates from which to resolve `CreditCard` or `PayPal`.

```typescript
// ✅ Good - explicit derivatives list
@field(Object, false, [CreditCard, PayPal])
paymentMethod!: IPaymentMethod;

// Missing runtime candidates for an erased interface
@field(Object)
paymentMethod!: IPaymentMethod;
```

A field typed with a concrete base class can omit the explicit list when each implementation uses `@derivedType`. The decorator automatically registers subclasses through the class inheritance chain.

### 5. Interface Consistency

Match the fields and JSON property names used by the backend contract. A TypeScript `number` can carry the example amount, but it does not provide .NET `decimal` precision semantics.

```typescript
// Frontend interface should match backend interface
export interface IPaymentMethod {
    amount: number; // matches decimal Amount in C#
}
```

### 6. Testing

Check both the reconstructed class and its data in a round-trip test. Include the identifier in the serialized result and a representative backend payload so that an ID mismatch is visible. The [JsonSerializer testing example](./json_serializer.md#2-test-round-trip-serialization) shows the basic round-trip assertions.

## Type System Integration

### Working with TypeScript's Type System

Once a payment field has been reconstructed, `instanceof` can distinguish the implementations and narrow their TypeScript types.

```typescript
// Type-safe polymorphic handling
function processPayment(payment: IPaymentMethod): void {
    if (payment instanceof CreditCard) {
        // TypeScript knows this is CreditCard
        console.log(`Processing card: ${payment.cardNumber}`);
    } else if (payment instanceof PayPal) {
        // TypeScript knows this is PayPal
        console.log(`Processing PayPal: ${payment.email}`);
    }
}

// Type guards for runtime checking
function isCreditCard(payment: IPaymentMethod): payment is CreditCard {
    return payment instanceof CreditCard;
}
```

### Generic Serialization

For an array whose elements all use one known constructor, pass that constructor to `deserializeArray`. The helper below does not select a different constructor for each root element; use a containing field like `PaymentHistory.transactions` for mixed implementations.

```typescript
// Generic deserialization
function deserializePayments<T extends IPaymentMethod>(
    json: string,
    targetType: Constructor<T>
): T[] {
    return JsonSerializer.deserializeArray(targetType, json);
}

const creditCards = deserializePayments(json, CreditCard);
```

<a id="runtime-type-safety"></a>

## Runtime Metadata and Validation

Runtime metadata tells `JsonSerializer` how to construct a model; it is not a schema validator for untrusted JSON. For example, the primitive field readers do not check that a supplied value is really a number or string. Validate incoming data separately when your application needs those checks. Use [runtime inspection](field_decorator.md#runtime-inspection) to examine the metadata used for reconstruction.

## Performance Considerations

Derived-type selection reads class metadata and searches the field's candidate list at runtime. Keep that list specific to the field's possible values; registering unrelated implementations adds candidates without helping resolution.

## Decorator Compatibility

`@derivedType` supports both legacy and standard decorators. `Fields` merges metadata recorded by both modes across inheritance, so base and derived classes may come from dependencies compiled with different decorator modes. A project may still choose one compiler mode for consistency. See [Decorator Modes](./field_decorator.md#decorator-modes) for the TypeScript and Babel configuration.

Fundamentals loads its metadata support when you import `derivedType`; you do not need a separate `reflect-metadata` import.

## See Also

Use [JsonSerializer](./json_serializer.md) to read and write the configured models. If a field needs a different runtime type or collection declaration, continue with the [field decorator guide](./field_decorator.md).
