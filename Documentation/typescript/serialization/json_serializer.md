# JsonSerializer

The `JsonSerializer` is the core utility for serializing and deserializing TypeScript objects to and from JSON. It works seamlessly with the `@field` decorator system and `@derivedType` decorators to provide type-safe, round-trip serialization that preserves runtime types and method access.

## Overview

The `JsonSerializer` bridges the gap between JSON data and strongly-typed TypeScript class instances, enabling:

- **True Object Deserialization**: JSON data becomes actual class instances, not plain objects
- **Method Preservation**: All class methods remain accessible on deserialized instances
- **Type Safety**: Automatic type resolution and validation during serialization/deserialization
- **Polymorphic Support**: Automatic handling of derived types and interface implementations
- **Seamless Backend Integration**: Compatible with .NET backend serialization format

## Key Benefits

### 1. Type-Safe Runtime Objects

Instead of plain JavaScript objects from `JSON.parse()`, the `JsonSerializer` creates actual class instances:

```typescript
// Traditional approach - loses type information
const json = `{"name":"John","age":30}`;
const user = JSON.parse(json); // Plain object, not a User

// With JsonSerializer - maintains type information
const user = JsonSerializer.deserialize(User, json); // Real User instance
console.log(user instanceof User); // ✅ true
```

### 2. Business Logic Access

Methods defined on your classes are immediately available on deserialized objects:

```typescript
export class User {
    @field(String)
    name!: string;
    
    @field(Number)
    age!: number;
    
    getDisplayName(): string {
        return `${this.name} (${this.age})`;
    }
}

const json = `{"name":"John","age":30}`;
const user = JsonSerializer.deserialize(User, json);

// Method available immediately
console.log(user.getDisplayName()); // ✅ "John (30)"
```

### 3. Automatic Polymorphic Resolution

When working with interfaces or base classes, the `JsonSerializer` automatically deserializes to the correct derived type:

```typescript
// Different payment types deserialized to their correct classes
@derivedType('credit-card-id')
export class CreditCard implements IPaymentMethod { }

@derivedType('paypal-id')
export class PayPal implements IPaymentMethod { }

const json = `{"method":{"amount":99.99,"_derivedTypeId":"credit-card-id"}}`;
const payment = JsonSerializer.deserialize(Payment, json);

console.log(payment.method instanceof CreditCard); // ✅ true
```

### 4. Proper Type Conversion

Complex types like `Date` and `Guid` are properly converted during deserialization:

```typescript
export class Order {
    @field(String)
    orderId!: string;
    
    @field(Date)
    createdAt!: Date;
    
    @field(Guid)
    customerId!: Guid;
}

const json = `{"orderId":"123","createdAt":"2023-01-15T10:30:00Z","customerId":"550e8400-e29b-41d4-a716-446655440000"}`;
const order = JsonSerializer.deserialize(Order, json);

console.log(order.createdAt instanceof Date); // ✅ true
console.log(order.customerId instanceof Guid); // ✅ true
```

## Usage

### Basic Serialization

Convert class instances to JSON:

```typescript
const user = new User();
user.name = "John";
user.age = 30;

const json = JsonSerializer.serialize(user);
console.log(json); // {"name":"John","age":30}
```

### Basic Deserialization

Convert JSON strings back to typed instances:

```typescript
const json = `{"name":"John","age":30}`;
const user = JsonSerializer.deserialize(User, json);

console.log(user instanceof User); // ✅ true
console.log(user.name); // "John"
```

### Array Deserialization

Handle arrays of typed objects:

```typescript
const json = `[
  {"name":"John","age":30},
  {"name":"Jane","age":25}
]`;

const users = JsonSerializer.deserializeArray(User, json);
// Array of User instances, not plain objects
```

## Integration with Field Decorators

The `JsonSerializer` relies on the `@field` decorator to understand your class structure. For comprehensive details about decorators, field configuration, and advanced patterns, see:

- [Field Decorator Documentation](./field_decorator.md) - Complete guide to the `@field` decorator system, runtime type safety, and advanced serialization patterns

## Integration with Derived Types

For polymorphic serialization and working with multiple implementations of the same interface, see:

- [DerivedTypes Serialization Documentation](./derived_types.md) - Full guide to the `@derivedType` decorator, polymorphic deserialization, and multi-type scenarios

## Best Practices

### 1. Complete Field Declaration

Ensure all serializable properties are decorated:

```typescript
// ✅ Good - all fields decorated
export class Product {
    @field(String)
    name!: string;
    
    @field(Number)
    price!: number;
    
    @field(Date)
    createdAt!: Date;
}
```

### 2. Test Round-Trip Serialization

Verify your objects serialize and deserialize correctly:

```typescript
const original = new User();
original.name = "John";
original.age = 30;

const json = JsonSerializer.serialize(original);
const deserialized = JsonSerializer.deserialize(User, json);

expect(deserialized.name).toBe(original.name);
expect(deserialized.age).toBe(original.age);
expect(deserialized.getDisplayName()).toBe(original.getDisplayName());
```

### 3. Handle Complex Nested Objects

The `JsonSerializer` automatically handles nested typed objects:

```typescript
export class Order {
    @field(String)
    orderId!: string;
    
    @field(Customer) // Nested typed object
    customer!: Customer;
    
    @field(Product, true) // Array of typed objects
    items!: Product[];
}

const order = JsonSerializer.deserialize(Order, json);
// customer is a Customer instance, items are Product instances
```

### 4. Error Handling

Wrap serialization calls in try-catch blocks for production code:

```typescript
try {
    const user = JsonSerializer.deserialize(User, jsonData);
    // Process user
} catch (error) {
    console.error('Deserialization failed:', error);
}
```

## Common Scenarios

### Backend Integration

Deserialize data received from your .NET backend:

```typescript
// Response from backend
const response = await fetch('/api/users/123');
const json = await response.json();

// Convert to typed instance
const user = JsonSerializer.deserialize(User, JSON.stringify(json));
```

### Working with Collections

```typescript
const usersJson = await (await fetch('/api/users')).json();
const users = JsonSerializer.deserializeArray(User, JSON.stringify(usersJson));
```

### Polymorphic API Responses

```typescript
// Single polymorphic field
const paymentJson = `{"method":{"amount":99.99,"_derivedTypeId":"..."}}`;
const payment = JsonSerializer.deserialize(Payment, paymentJson);

// Array of polymorphic types
const methodsJson = `[
  {"amount":50,"_derivedTypeId":"credit-card"},
  {"amount":40,"_derivedTypeId":"paypal"}
]`;
const methods = JsonSerializer.deserializeArray(IPaymentMethod, methodsJson);
```

## See Also

- [Field Decorator](./field_decorator.md) - Decorator system for field serialization configuration
- [Derived Types](./derived_types.md) - Polymorphic type handling and interface implementations
- [Serialization Overview](./index.md) - Complete serialization system documentation
