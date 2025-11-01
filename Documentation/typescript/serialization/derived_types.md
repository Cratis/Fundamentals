# DerivedTypes Serialization (Frontend)

The DerivedTypes serialization system in the TypeScript frontend provides seamless polymorphic serialization that works hand-in-hand with the backend .NET implementation. This system allows you to deserialize JSON objects into their correct TypeScript class instances based on type identifiers.

> **Note**: This documentation focuses on derived types and polymorphic serialization. For comprehensive information about the `@field` decorator, including runtime type safety benefits and detailed usage patterns, see [Field Decorator Documentation](field_decorator.md).

## Overview

The frontend DerivedTypes system consists of:

- **`@derivedType` decorator**: Marks classes with unique identifiers matching the backend
- **`DerivedType` class**: Manages metadata for derived type identifiers
- **`JsonSerializer`**: Handles serialization/deserialization with type information
- **`@field` decorator**: Defines fields with optional derivative type support
- **`Field` class**: Represents field metadata including derivative type information

## Key Concepts

### Type Metadata System

The frontend uses TypeScript decorators and reflect-metadata to store type information:

- **Derived Type IDs**: Unique string identifiers that must match exactly with backend `[DerivedType]` attributes
- **Field Metadata**: Information about properties including their types and possible derivatives
- **Runtime Type Resolution**: Dynamic instantiation of correct classes during deserialization

### Decorator-Based Configuration

Unlike the backend's attribute-based approach, the frontend uses decorators to mark classes and fields with metadata.

## Uniqueness Requirements

Derived type IDs must be **unique per interface** to enable proper polymorphic deserialization. While you can use any string format, using descriptive identifiers is recommended for readability:

**Good approach** - Descriptive and unique:

```typescript
@derivedType('credit-card')
export class CreditCard implements IPaymentMethod { }

@derivedType('paypal')
export class PayPal implements IPaymentMethod { }
```

**Alternative approach** - Using GUIDs for maximum uniqueness:

```typescript
@derivedType('550e8400-e29b-41d4-a716-446655440001')
export class CreditCard implements IPaymentMethod { }

@derivedType('550e8400-e29b-41d4-a716-446655440002')
export class PayPal implements IPaymentMethod { }
```

> **Note on GUIDs**: While GUIDs guarantee uniqueness, they sacrifice readability. If using GUIDs, ensure they are stored in shared constants or configuration to maintain consistency across your codebase and backend services.

## Usage

### 1. Define Your Interface

```typescript
export interface IPaymentMethod {
    amount: number;
}
```

### 2. Create Derived Types

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

## Serialization and Deserialization

For detailed information about serialization and deserialization operations, including usage examples and API details, see the [JsonSerializer Documentation](./json_serializer.md).

When working with polymorphic types, the `JsonSerializer` automatically handles:

- Adding `_derivedTypeId` during serialization based on the `@derivedType` decorator
- Using `_derivedTypeId` to instantiate the correct derived type during deserialization

## Field Decorator Integration

The `@field` decorator is essential for defining serializable properties. For comprehensive documentation on the `@field` decorator including all parameters, usage patterns, and runtime type safety benefits, see [Field Decorator Documentation](field_decorator.md).

## Serialization Format

The frontend produces the same JSON format as the backend:

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

The `_derivedTypeId` property is automatically:

- **Added during serialization** based on the class's `@derivedType` decorator
- **Used during deserialization** to instantiate the correct class

## Advanced Scenarios

### Complex Nested Objects

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

```typescript
export class PaymentHistory {
    @field(Object, true, [CreditCard, PayPal, BankTransfer])
    transactions!: IPaymentMethod[];
}
```

### Well-Known Types

The system automatically handles common types:

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

1. **Missing `@derivedType` decorator**: Classes won't be recognized during deserialization
2. **Mismatched IDs**: Frontend and backend derived type IDs must match exactly
3. **Missing field decorators**: Properties without `@field` won't be serialized/deserialized
4. **Missing derivatives list**: Polymorphic fields need the derivatives parameter

### Debugging Tips

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

> **Why centralize?** These "magic strings" are critical for serialization consistency. Centralizing them in one place prevents duplicates, makes them easy to maintain, and ensures frontend and backend stay synchronized.

### 3. Complete Field Decoration

Decorate all serializable properties:

```typescript
export class CreditCard implements IPaymentMethod {
    @field(Number)
    amount!: number;
    
    @field(String)
    cardNumber!: string;
    
    // ❌ This won't be serialized without @field
    private internalId: string = '';
    
    // ✅ Private fields that should be serialized need @field too
    @field(String)
    private securityCode!: string;
}
```

### 4. Explicit Derivative Lists

Always specify derivatives for polymorphic fields:

```typescript
// ✅ Good - explicit derivatives list
@field(Object, false, [CreditCard, PayPal])
paymentMethod!: IPaymentMethod;

// ❌ Bad - missing derivatives, won't deserialize correctly
@field(Object)
paymentMethod!: IPaymentMethod;
```

### 5. Interface Consistency

Keep interfaces synchronized between frontend and backend:

```typescript
// Frontend interface should match backend interface
export interface IPaymentMethod {
    amount: number; // matches decimal Amount in C#
}
```

### 6. Testing

Test serialization round-trips:

```typescript
## Testing

For testing serialization round-trips with derived types, create tests that verify:

1. Polymorphic types deserialize to the correct class
2. Derived type IDs are preserved during serialization
3. Frontend IDs match backend IDs exactly

Refer to the [JsonSerializer Documentation](./json_serializer.md) for comprehensive testing examples.
```

## Type System Integration

### Working with TypeScript's Type System

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

## Runtime Type Safety

The `@field` decorator system provides significant runtime type safety benefits beyond TypeScript's compile-time checking. For detailed information about runtime type safety, including comprehensive examples and comparisons, see the [Field Decorator Documentation](field_decorator.md#runtime-type-safety-benefits).

With derived types, you benefit from:

- Automatic polymorphic type resolution at runtime
- Correct class instantiation based on `_derivedTypeId`
- Full method access on all polymorphic instances
- Type-safe instanceof checks across derived types

## Performance Considerations

- **Reflection overhead**: Metadata lookup happens at runtime
- **Type scanning**: Keep derivative lists focused to avoid unnecessary type checking
- **Memory usage**: Metadata is stored per-class, not per-instance

## Integration with Build Tools

### TypeScript Configuration

Ensure your `tsconfig.json` includes:

```json
{
  "compilerOptions": {
    "experimentalDecorators": true,
    "emitDecoratorMetadata": true
  }
}
```

### Bundling Considerations

When using module bundlers, ensure reflect-metadata is properly included:

```typescript
import 'reflect-metadata';
// Import this before any decorated classes
```

## See Also

- [JsonSerializer](./json_serializer.md) - Core serialization utility for type-safe JSON conversion
- [Field Decorator](./field_decorator.md) - Comprehensive guide to the `@field` decorator system and runtime type safety

