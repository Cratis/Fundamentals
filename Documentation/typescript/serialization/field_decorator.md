# @field Decorator

The `@field` decorator is a fundamental component of the TypeScript serialization system that provides runtime type information and enables proper object deserialization with full type safety.

## Overview

The `@field` decorator system transforms TypeScript's compile-time type information into runtime metadata, enabling runtime type safety and proper deserialization. It works seamlessly with the `JsonSerializer` for complete type-safe serialization.

Key capabilities:

- **Runtime Type Metadata**: Compile-time types converted to runtime metadata
- **Proper Deserialization**: Works with `JsonSerializer` to create actual class instances
- **Type-Safe Collections**: Proper typing for arrays and complex nested objects
- **Deep Object Graphs**: Maintaining correct types throughout complex object hierarchies

## Basic Usage

### Simple Field Declaration

```typescript
import { field } from '../fieldDecorator';

export class User {
    @field(String)
    name!: string;
    
    @field(Number)
    age!: number;
    
    @field(Boolean)
    isActive!: boolean;
    
    @field(Date)
    createdAt!: Date;
    
    // Method available on deserialized instances
    getDisplayName(): string {
        return `${this.name} (${this.age})`;
    }
}
```

### Decorator Parameters

The `@field` decorator accepts three parameters:

```typescript
@field(targetType: Constructor, enumerable?: boolean, derivatives?: Constructor[])
```

- **`targetType`**: The type constructor for the field (String, Number, Date, custom classes, etc.)
- **`enumerable`**: Set to `true` for arrays/collections (default: `false`)
- **`derivatives`**: Array of possible derived types for polymorphic fields (default: `[]`)

## Field Types

### Primitive Types

```typescript
export class Product {
    @field(String)
    name!: string;
    
    @field(Number)
    price!: number;
    
    @field(Boolean)
    inStock!: boolean;
    
    @field(Date)
    lastUpdated!: Date;
    
    @field(Guid)
    id!: Guid;
}
```

### Array Fields

Use the `enumerable: true` parameter for arrays:

```typescript
export class ShoppingCart {
    @field(String, true) // Array of strings
    tags!: string[];
    
    @field(Number, true) // Array of numbers
    quantities!: number[];
    
    @field(Product, true) // Array of custom objects
    items!: Product[];
}
```

### Complex Objects

```typescript
export class Order {
    @field(String)
    orderId!: string;
    
    @field(User) // Nested object
    customer!: User;
    
    @field(Product, true) // Array of objects
    items!: Product[];
    
    @field(Date)
    orderDate!: Date;
    
    calculateTotal(): number {
        return this.items.reduce((sum, item) => sum + item.price, 0);
    }
}
```

### Polymorphic Fields

For interfaces or base classes with multiple implementations, specify the possible derived types:

```typescript
export interface IPaymentMethod {
    amount: number;
}

@derivedType('credit-card-v1')
export class CreditCard implements IPaymentMethod {
    @field(Number)
    amount!: number;
    
    @field(String)
    cardNumber!: string;
}

@derivedType('paypal-v1')
export class PayPal implements IPaymentMethod {
    @field(Number)
    amount!: number;
    
    @field(String)
    email!: string;
}

export class Payment {
    @field(Object, false, [CreditCard, PayPal]) // Single polymorphic field
    method!: IPaymentMethod;
    
    @field(Object, true, [CreditCard, PayPal]) // Array of polymorphic objects
    backupMethods!: IPaymentMethod[];
}
```

## Runtime Type Safety Benefits

### Traditional JavaScript Object Limitations

Without the `@field` decorator system, JSON deserialization creates plain JavaScript objects:

```typescript
// Traditional approach - no runtime type safety
const json = `{"name": "John", "age": 30, "createdAt": "2023-01-01T00:00:00Z"}`;
const user = JSON.parse(json) as User;

console.log(user.constructor.name); // "Object" - not User!
console.log(user instanceof User); // false
user.getDisplayName(); // ❌ Runtime error - method doesn't exist
typeof user.createdAt; // "string" - not Date object
```

### With @field Decorator - True Runtime Types

The `@field` decorator system creates actual class instances with full method access:

```typescript
// With @field system - full runtime type safety
const json = `{"name": "John", "age": 30, "createdAt": "2023-01-01T00:00:00Z"}`;
const user = JsonSerializer.deserialize(User, json);

console.log(user.constructor.name); // "User" ✅
console.log(user instanceof User); // true ✅
console.log(user.getDisplayName()); // "John (30)" ✅ - method available
console.log(user.createdAt instanceof Date); // true ✅ - proper Date object
```

### Polymorphic Runtime Type Resolution

The system automatically resolves derived types at runtime based on the `_derivedTypeId`:

```typescript
export class PaymentProcessor {
    @field(Object, true, [CreditCard, PayPal])
    supportedMethods!: IPaymentMethod[];
    
    processPayments(): void {
        this.supportedMethods.forEach(method => {
            // Each method is the correct runtime type
            if (method instanceof CreditCard) {
                // Full CreditCard methods available
                console.log(`Processing card: ${method.cardNumber}`);
                method.validateCard(); // Method available at runtime
            } else if (method instanceof PayPal) {
                // Full PayPal methods available
                console.log(`Processing PayPal: ${method.email}`);
                method.validateEmail(); // Method available at runtime
            }
        });
    }
}
```

### Deep Object Graphs with Runtime Types

Complex nested objects maintain their proper types throughout the object graph:

```typescript
export class Customer {
    @field(String)
    name!: string;
    
    @field(Order, true)
    orders!: Order[];
    
    @field(Object, false, [CreditCard, PayPal])
    defaultPayment!: IPaymentMethod;
    
    getTotalSpent(): number {
        // All nested objects are proper runtime types
        return this.orders.reduce((total, order) => total + order.calculateTotal(), 0);
    }
    
    hasValidPayment(): boolean {
        return this.defaultPayment instanceof CreditCard 
            ? this.defaultPayment.validateCard()
            : this.defaultPayment.validateEmail();
    }
}
```

### Data Validation and Business Rules

Runtime types enable rich validation and business logic on deserialized data:

```typescript
export class BankAccount {
    @field(String)
    accountNumber!: string;
    
    @field(String)
    routingNumber!: string;
    
    @field(Number)
    balance!: number;
    
    @field(Date)
    lastTransaction!: Date;
    
    validateAccountNumber(): boolean {
        // Complex validation logic available on runtime instances
        return this.accountNumber.length >= 4 && this.accountNumber.length <= 17;
    }
    
    validateRoutingNumber(): boolean {
        return /^\d{9}$/.test(this.routingNumber);
    }
    
    canWithdraw(amount: number): boolean {
        return this.balance >= amount && this.validateAccountNumber();
    }
    
    getAccountAge(): number {
        // Business logic methods work on deserialized instances
        const now = new Date();
        return Math.floor((now.getTime() - this.lastTransaction.getTime()) / (1000 * 60 * 60 * 24));
    }
}

// Usage with deserialized data
const accountData = `{
    "accountNumber": "1234567890",
    "routingNumber": "123456789",
    "balance": 1500.50,
    "lastTransaction": "2023-10-01T10:30:00Z"
}`;

const account = JsonSerializer.deserialize(BankAccount, accountData);

// All methods work on the deserialized instance
console.log(account.validateAccountNumber()); // true
console.log(account.canWithdraw(1000)); // true
console.log(account.getAccountAge()); // Number of days
console.log(account.lastTransaction instanceof Date); // true
```

## Serialization and Deserialization

For detailed information about serialization and deserialization operations, see the [JsonSerializer Documentation](./json_serializer.md).

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
    return payment instanceof CreditCard; // Works with runtime types
}
```

### Generic Serialization

```typescript
// Generic deserialization helper
function deserializeCollection<T extends object>(
    json: string,
    targetType: Constructor<T>
): T[] {
    return JsonSerializer.deserializeArray(targetType, json);
}

const products = deserializeCollection(json, Product);
const users = deserializeCollection(userJson, User);
```

## Best Practices

### 1. Complete Field Decoration

Decorate all serializable properties:

```typescript
export class Product {
    @field(String)
    name!: string;
    
    @field(Number)
    price!: number;
    
    // ❌ This won't be serialized without @field
    private internalId: string = '';
    
    // ✅ Private fields that should be serialized need @field too
    @field(String)
    private sku!: string;
    
    // ✅ Methods don't need @field - they're part of the class
    calculateTax(): number {
        return this.price * 0.08;
    }
}
```

### 2. Explicit Type Specification

Always specify the correct target type:

```typescript
// ✅ Good - explicit type
@field(Date)
createdAt!: Date;

@field(Guid)
id!: Guid;

// ✅ Good - explicit derivatives for polymorphic fields
@field(Object, false, [CreditCard, PayPal])
paymentMethod!: IPaymentMethod;

// ❌ Bad - missing derivatives, won't deserialize correctly
@field(Object)
paymentMethod!: IPaymentMethod;
```

### 3. Array Handling

Use the enumerable parameter correctly:

```typescript
// ✅ Good - enumerable: true for arrays
@field(String, true)
tags!: string[];

@field(Product, true)
items!: Product[];

// ❌ Bad - missing enumerable parameter
@field(String)
tags!: string[]; // Won't deserialize array properly
```

### 4. Testing Serialization Round-trips

Always test serialization and deserialization:

```typescript
describe('Product serialization', () => {
    it('should serialize and deserialize correctly', () => {
        const original = new Product();
        original.name = "Test Product";
        original.price = 99.99;
        
        const json = JsonSerializer.serialize(original);
        const deserialized = JsonSerializer.deserialize(Product, json);
        
        expect(deserialized.constructor).toBe(Product);
        expect(deserialized.name).toBe("Test Product");
        expect(deserialized.price).toBe(99.99);
        expect(deserialized.calculateTax()).toBe(7.9992); // Method works
    });
});
```

## Comparison: With vs Without @field Decorator

| Aspect | Without @field | With @field |
|--------|---------------|-------------|
| **Runtime Type** | Plain Object | Actual Class Instance |
| **instanceof checks** | ❌ Always false | ✅ Works correctly |
| **Method access** | ❌ Runtime errors | ✅ Full method access |
| **Polymorphism** | ❌ Manual type checking | ✅ Automatic resolution |
| **Type safety** | ❌ Compile-time only | ✅ Runtime + compile-time |
| **Business logic** | ❌ External functions | ✅ Encapsulated methods |
| **Validation** | ❌ Manual/external | ✅ Built-in methods |
| **Debugging** | ❌ Generic objects | ✅ Named class instances |
| **Date handling** | ❌ Strings | ✅ Proper Date objects |
| **Nested objects** | ❌ Plain objects | ✅ Proper class instances |

## Error Handling

### Common Issues

1. **Missing @field decorator**: Properties without `@field` won't be serialized/deserialized
2. **Wrong target type**: Incorrect type specification leads to deserialization errors
3. **Missing enumerable flag**: Arrays won't deserialize properly without `enumerable: true`
4. **Missing derivatives**: Polymorphic fields need the derivatives parameter

### Debugging Tips

```typescript
// Check field metadata for a type
const fields = Fields.getFieldsForType(Product);
console.log('Product fields:', fields);

// Verify field configuration
fields.forEach(field => {
    console.log(`Field: ${field.name}, Type: ${field.type.name}, Enumerable: ${field.enumerable}`);
});
```

## Performance Considerations

- **Metadata overhead**: Field metadata is stored per-class, not per-instance
- **Reflection cost**: Type resolution happens during deserialization
- **Memory efficiency**: Keep derivative lists focused to relevant types only
- **Lazy loading**: Consider lazy initialization for heavy objects

## Integration Requirements

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

### Runtime Dependencies

```typescript
import 'reflect-metadata';
// Import this before any decorated classes
```

The `@field` decorator system provides a robust foundation for type-safe serialization in TypeScript applications, working together with the `JsonSerializer` to bridge the gap between compile-time type safety and runtime object integrity.

## See Also

- [JsonSerializer](./json_serializer.md) - Core serialization utility for type-safe JSON conversion
- [Derived Types](./derived_types.md) - Polymorphic type handling and interface implementations

