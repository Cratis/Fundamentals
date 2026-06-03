# ConceptAs

The `ConceptAs<T>` abstract class provides a TypeScript equivalent to the C# `ConceptAs<T>`, allowing you to create strongly-typed domain identifiers and value objects that wrap primitive types such as `string`, `number`, or `boolean`.

## Overview

ConceptAs is part of Domain-Driven Design (DDD) patterns for creating ubiquitous language in your codebase. Instead of using raw primitive types like `string` or `number` for domain-specific values, you create explicit types that better express their purpose and prevent mix-ups.

## Key Benefits

### 1. Type Safety

Instead of passing around generic strings or numbers, you create strongly-typed domain concepts:

```typescript
// Without ConceptAs - prone to errors
function getUser(id: string) { ... }
function getOrder(id: string) { ... }

// With ConceptAs - compile-time safety
class UserId extends ConceptAs<string> {}
class OrderId extends ConceptAs<string> {}

function getUser(id: UserId) { ... }
function getOrder(id: OrderId) { ... }

// This would be a compile-time error:
const userId = new UserId("user-123");
getOrder(userId); // ❌ Type error - OrderId expected
```

### 2. Self-Documenting Code

The type names make the code more readable and self-explanatory:

```typescript
// Before: What does this string represent?
function createInvoice(customerId: string, amount: number) { ... }

// After: Clear semantic meaning
class CustomerId extends ConceptAs<string> {}
class InvoiceAmount extends ConceptAs<number> {}

function createInvoice(customerId: CustomerId, amount: InvoiceAmount) { ... }
```

### 3. JSON Serialization Support

ConceptAs types integrate seamlessly with the `JsonSerializer`, automatically serializing to their inner values and deserializing back to typed instances:

```typescript
import { ConceptAs, JsonSerializer, field } from '@cratis/fundamentals';

class UserId extends ConceptAs<string> {}

class User {
    @field(UserId)
    id!: UserId;
    
    @field(String)
    name!: string;
}

// Serialization
const user = new User();
user.id = new UserId('user-123');
user.name = 'John Doe';

const json = JsonSerializer.serialize(user);
// Result: {"id":"user-123","name":"John Doe"}

// Deserialization
const deserialized = JsonSerializer.deserialize(User, json);
console.log(deserialized.id instanceof UserId); // ✅ true
console.log(deserialized.id.value); // "user-123"
```

## Usage

### Creating a ConceptAs Type

To create a ConceptAs type, extend the `ConceptAs<T>` abstract class with the appropriate primitive type:

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserId extends ConceptAs<string> {}
class OrderCount extends ConceptAs<number> {}
class IsActive extends ConceptAs<boolean> {}
```

### Using ConceptAs Types

Instantiate your concept types by passing the underlying value:

```typescript
const userId = new UserId('user-123');
const orderCount = new OrderCount(42);
const isActive = new IsActive(true);

// Access the underlying value
console.log(userId.value); // "user-123"
console.log(orderCount.value); // 42

// Get primitive value for operations
console.log(userId.valueOf()); // "user-123"

// String representation
console.log(userId.toString()); // "user-123"
console.log(orderCount.toString()); // "42"
```

### Using with JsonSerializer

To use ConceptAs types in objects that will be serialized/deserialized, mark the fields with the `@field` decorator:

```typescript
import { ConceptAs, JsonSerializer, field } from '@cratis/fundamentals';

class UserId extends ConceptAs<string> {}
class OrderId extends ConceptAs<string> {}

class Order {
    @field(OrderId)
    orderId!: OrderId;
    
    @field(UserId)
    customerId!: UserId;
    
    @field(Number)
    totalAmount!: number;
}

// Create and serialize
const order = new Order();
order.orderId = new OrderId('order-456');
order.customerId = new UserId('user-123');
order.totalAmount = 99.99;

const json = JsonSerializer.serialize(order);
console.log(json);
// {"orderId":"order-456","customerId":"user-123","totalAmount":99.99}

// Deserialize back to typed instances
const restored = JsonSerializer.deserialize(Order, json);
console.log(restored.orderId instanceof OrderId); // ✅ true
console.log(restored.customerId instanceof UserId); // ✅ true
```

## API Reference

### Constructor

```typescript
constructor(readonly value: T)
```

Creates a new instance of the concept with the specified value.

**Parameters:**
- `value: T` - The underlying value to wrap

**Example:**
```typescript
class UserId extends ConceptAs<string> {}
const userId = new UserId('user-123');
```

### Properties

#### `value: T` (readonly)

The underlying primitive value.

**Example:**
```typescript
const userId = new UserId('user-123');
console.log(userId.value); // "user-123"
```

### Methods

#### `valueOf(): T`

Returns the primitive value of the concept. This method is used by JavaScript when automatic type coercion is needed.

**Returns:** The underlying value

**Example:**
```typescript
const count = new OrderCount(42);
console.log(count.valueOf()); // 42
console.log(count + 10); // 52 (automatic coercion)
```

#### `toString(): string`

Returns the string representation of the concept.

**Returns:** String representation of the underlying value

**Example:**
```typescript
const userId = new UserId('user-123');
console.log(userId.toString()); // "user-123"

const count = new OrderCount(42);
console.log(count.toString()); // "42"
```

## Best Practices

### 1. Use for Domain Identifiers

Create ConceptAs types for all domain identifiers:

```typescript
class UserId extends ConceptAs<string> {}
class ProductId extends ConceptAs<string> {}
class OrderId extends ConceptAs<string> {}
class CustomerId extends ConceptAs<string> {}
```

### 2. Use for Measured Values

Wrap numeric values that represent specific measurements or counts:

```typescript
class OrderCount extends ConceptAs<number> {}
class Price extends ConceptAs<number> {}
class Quantity extends ConceptAs<number> {}
class TemperatureInCelsius extends ConceptAs<number> {}
```

### 3. Don't Add Extra Properties

> ⚠️ **Important**: ConceptAs types are meant to wrap a single primitive value only. Don't add additional properties to ConceptAs subclasses, as the JsonSerializer assumes they only contain the `value` property.

```typescript
// ❌ DON'T do this
class UserId extends ConceptAs<string> {
    createdAt: Date; // This will break serialization
}

// ✅ DO this instead - create a separate class
class User {
    @field(UserId)
    id!: UserId;
    
    @field(Date)
    createdAt!: Date;
}
```

### 4. Keep Concepts Simple

ConceptAs is for wrapping primitives. For complex domain objects with multiple properties, create regular classes instead.

## Comparison with C# ConceptAs

The TypeScript implementation mirrors the C# version but with JavaScript idioms:

| Feature | C# | TypeScript |
|---------|-----|-----------|
| Base class | `ConceptAs<T>` record | `ConceptAs<T>` abstract class |
| Value property | `Value` property | `value` readonly property |
| Implicit conversion | `implicit operator T` | `valueOf()` method |
| String representation | `ToString()` override | `toString()` method |
| JSON serialization | `ConceptAsJsonConverter` | Built into `JsonSerializer` |
| Comparison operators | `IComparable` interface | Not implemented (use `value` directly) |

## See Also

- [JsonSerializer](./serialization/json_serializer.md) - JSON serialization with ConceptAs support
- [Field Decorator](./serialization/field_decorator.md) - Declaring fields for serialization
- [C# Concepts](../csharp/concepts.md) - C# equivalent documentation
