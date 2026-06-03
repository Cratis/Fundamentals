# ConceptAs

The `ConceptAs<T>` abstract class provides a TypeScript equivalent to the C# `ConceptAs<T>`, allowing you to create strongly-typed domain identifiers and value objects that wrap primitive types such as `string`, `number`, or `boolean`.

The recommended pattern is to export concepts as union types for convenient assignment of both concept instances and primitives:

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;
```

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
class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderIdConcept extends ConceptAs<string> {}
export type OrderId = OrderIdConcept | string;

function getUser(id: UserId) { ... }
function getOrder(id: OrderId) { ... }

// This would be a compile-time error:
const userId = new UserIdConcept("user-123");
getOrder(userId); // ❌ Type error - OrderId expected
```

### 2. Self-Documenting Code

The type names make the code more readable and self-explanatory:

```typescript
// Before: What does this string represent?
function createInvoice(customerId: string, amount: number) { ... }

// After: Clear semantic meaning
class CustomerIdConcept extends ConceptAs<string> {}
export type CustomerId = CustomerIdConcept | string;

class InvoiceAmountConcept extends ConceptAs<number> {}
export type InvoiceAmount = InvoiceAmountConcept | number;

function createInvoice(customerId: CustomerId, amount: InvoiceAmount) { ... }
```

### 3. JSON Serialization Support

ConceptAs types integrate seamlessly with the `JsonSerializer`, automatically serializing to their inner values and deserializing back to typed instances:

```typescript
import { ConceptAs, JsonSerializer, field } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class User {
    @field(UserIdConcept)
    id!: UserId;
    
    @field(String)
    name!: string;
}

// Serialization
const user = new User();
user.id = new UserIdConcept('user-123'); // or just 'user-123'
user.name = 'John Doe';

const json = JsonSerializer.serialize(user);
// Result: {"id":"user-123","name":"John Doe"}

// Deserialization
const deserialized = JsonSerializer.deserialize(User, json);
console.log(deserialized.id instanceof UserIdConcept); // ✅ true
console.log((deserialized.id as UserIdConcept).value); // "user-123"
```

## Usage

### Creating a ConceptAs Type

To create a ConceptAs type, extend the `ConceptAs<T>` abstract class with the appropriate primitive type. For convenience and ease of reading, export the concept as a union type that includes both the concept class and the underlying primitive:

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;

class IsActiveConcept extends ConceptAs<boolean> {}
export type IsActive = IsActiveConcept | boolean;
```

This pattern provides:
- **Convenience**: Consumers can assign either the concept instance or the primitive value
- **Readability**: The exported type name is clean and matches domain language
- **Type Safety**: The `@field` decorator metadata ensures correct serialization/deserialization

### Using ConceptAs Types

Instantiate your concept types by passing the underlying value. With the union type export pattern, you can assign either concept instances or primitive values:

```typescript
// Instantiate the concept class
const userId = new UserIdConcept('user-123');
const orderCount = new OrderCountConcept(42);
const isActive = new IsActiveConcept(true);

// Access the underlying value
console.log(userId.value); // "user-123"
console.log(orderCount.value); // 42

// Get primitive value for operations
console.log(userId.valueOf()); // "user-123"

// String representation
console.log(userId.toString()); // "user-123"
console.log(orderCount.toString()); // "42"

// With union types, you can also assign primitives directly
let flexibleUserId: UserId = 'user-456'; // Works!
flexibleUserId = new UserIdConcept('user-789'); // Also works!
```

### Using with JsonSerializer

To use ConceptAs types in objects that will be serialized/deserialized, mark the fields with the `@field` decorator. The `@field` decorator takes the concept class (not the union type), ensuring proper deserialization:

```typescript
import { ConceptAs, JsonSerializer, field } from '@cratis/fundamentals';

// Define concepts with union type exports
class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderIdConcept extends ConceptAs<string> {}
export type OrderId = OrderIdConcept | string;

class Order {
    // @field takes the class, type annotation uses the exported union type
    @field(OrderIdConcept)
    orderId!: OrderId;
    
    @field(UserIdConcept)
    customerId!: UserId;
    
    @field(Number)
    totalAmount!: number;
}

// Create and serialize - can use either concept instances or primitives
const order = new Order();
order.orderId = new OrderIdConcept('order-456'); // or 'order-456'
order.customerId = 'user-123'; // or new UserIdConcept('user-123')
order.totalAmount = 99.99;

const json = JsonSerializer.serialize(order);
console.log(json);
// {"orderId":"order-456","customerId":"user-123","totalAmount":99.99}

// Deserialize back to typed instances
const restored = JsonSerializer.deserialize(Order, json);
console.log(restored.orderId instanceof OrderIdConcept); // ✅ true
console.log(restored.customerId instanceof UserIdConcept); // ✅ true
```

> 🔑 **Key Point**: The `@field` decorator always references the concept class (e.g., `UserIdConcept`), while the TypeScript type annotation uses the exported union type (e.g., `UserId`). The `@field` metadata ensures proper deserialization to the concept class, while the union type allows flexible assignment.

### Union Types for Flexible Assignment

To make it convenient to work with ConceptAs types, the recommended pattern is to export the concept as a union type that includes both the concept class and the underlying primitive. This allows both ConceptAs instances and primitive values to be assigned to the same field, providing flexibility while maintaining type safety and proper serialization.

#### Recommended Export Pattern

Define your concept file with both the concept class and the union type export:

```typescript
// UserId.ts
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}

export type UserId = UserIdConcept | string;
```

```typescript
// OrderCount.ts
import { ConceptAs } from '@cratis/fundamentals';

class OrderCountConcept extends ConceptAs<number> {}

export type OrderCount = OrderCountConcept | number;
```

**Benefits of this pattern:**
- **Convenience**: Consumers can assign either the concept instance or the primitive value
- **Readability**: The exported type name (`UserId`) is clean and matches domain language
- **Encapsulation**: The concept class name (`UserIdConcept`) is an implementation detail
- **Consistency**: All concept files follow the same export pattern

#### Using the Exported Types

When declaring fields, import and use the exported union type. The `@field` decorator still references the concept class for proper serialization:

```typescript
import { JsonSerializer, field } from '@cratis/fundamentals';
import { UserId, UserIdConcept } from './UserId';
import { OrderCount, OrderCountConcept } from './OrderCount';

class Order {
    // @field takes the class, type annotation uses the exported union type
    @field(UserIdConcept)
    userId!: UserId;
    
    @field(OrderCountConcept)
    orderCount!: OrderCount;
    
    @field(String)
    description!: string;
}
```

> 🔑 **Key Point**: The `@field` decorator specifies the concept class (e.g., `UserIdConcept`) for serialization metadata, while the TypeScript type annotation uses the exported union type (e.g., `UserId`) for flexible assignment.

#### Assignment Flexibility

With union types exported from the concept file, you can assign either concept instances or primitive values:

```typescript
const order = new Order();

// Option 1: Assign concept instances
order.userId = new UserIdConcept('user-123');
order.orderCount = new OrderCountConcept(42);

// Option 2: Assign primitive values directly
order.userId = 'user-456';
order.orderCount = 99;

// Option 3: Mix both approaches
order.userId = new UserIdConcept('user-789');
order.orderCount = 55; // primitive
```

#### Serialization Behavior

The JsonSerializer handles both ConceptAs instances and primitives correctly, thanks to the `@field` decorator metadata:

**Serialization** - Both ConceptAs instances and primitives serialize to the inner primitive value:

```typescript
// With ConceptAs instances
const order1 = new Order();
order1.userId = new UserIdConcept('user-123');
order1.orderCount = new OrderCountConcept(42);

const json1 = JsonSerializer.serialize(order1);
// Result: {"userId":"user-123","orderCount":42}

// With primitives
const order2 = new Order();
order2.userId = 'user-456';
order2.orderCount = 99;

const json2 = JsonSerializer.serialize(order2);
// Result: {"userId":"user-456","orderCount":99}
```

**Deserialization** - Always deserializes to ConceptAs instances based on the `@field` decorator:

```typescript
const json = '{"userId":"user-123","orderCount":42}';
const order = JsonSerializer.deserialize(Order, json);

console.log(order.userId instanceof UserIdConcept); // ✅ true
console.log(order.orderCount instanceof OrderCountConcept); // ✅ true
console.log((order.userId as UserIdConcept).value); // "user-123"
console.log((order.orderCount as OrderCountConcept).value); // 42
```

> 📝 **Note**: Regardless of whether you assigned a ConceptAs instance or a primitive during serialization, deserialization always creates ConceptAs instances based on the concept class specified in the `@field` decorator.

#### Implementation Details

The JsonSerializer follows the C# pattern for handling ConceptAs types:

1. **Recognition**: During serialization, the serializer detects ConceptAs instances
2. **Unwrapping**: Extracts the inner value from the ConceptAs instance
3. **Recursive Serialization**: Calls the serializer recursively on the inner value to handle complex types properly

This ensures that:
- Simple primitives (string, number, boolean) are serialized directly
- Complex inner types are properly serialized using their own serialization logic
- Union types work seamlessly without special handling

#### Best Practices for Union Types

**✅ DO** export union types from the concept file for convenience and consistency:

```typescript
// UserId.ts
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}

export type UserId = UserIdConcept | string;
```

**✅ DO** use union types in method signatures for flexible APIs:

```typescript
import { UserId, UserIdConcept } from './UserId';
import { OrderCount, OrderCountConcept } from './OrderCount';

class OrderService {
    // Flexible method signature accepts both concept instances and primitives
    createOrder(userId: UserId, count: OrderCount) {
        const order = new Order();
        order.userId = userId; // Works with both types
        order.orderCount = count;
        return order;
    }
}

// Can be called both ways
service.createOrder(new UserIdConcept('user-123'), new OrderCountConcept(42));
service.createOrder('user-123', 42);
```

**✅ DO** use type guards when you need to distinguish between ConceptAs and primitives:

```typescript
import { UserId, UserIdConcept } from './UserId';

function processUserId(userId: UserId) {
    if (userId instanceof UserIdConcept) {
        // Handle ConceptAs instance
        console.log('UserId:', userId.value);
    } else {
        // Handle primitive string
        console.log('String:', userId);
    }
}
```

**❌ DON'T** forget the `@field` decorator - it's required for proper deserialization:

```typescript
// ❌ BAD: Missing @field decorator
class Order {
    userId!: UserId; // Won't deserialize correctly!
}

// ✅ GOOD: Has @field decorator with concept class
class Order {
    @field(UserIdConcept)
    userId!: UserId;
}
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
class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

const userId = new UserIdConcept('user-123');
```

### Properties

#### `value: T` (readonly)

The underlying primitive value.

**Example:**
```typescript
const userId = new UserIdConcept('user-123');
console.log(userId.value); // "user-123"
```

### Methods

#### `valueOf(): T`

Returns the primitive value of the concept. This method is used by JavaScript when automatic type coercion is needed.

**Returns:** The underlying value

**Example:**
```typescript
class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;

const count = new OrderCountConcept(42);
console.log(count.valueOf()); // 42
console.log(count + 10); // 52 (automatic coercion)
```

#### `toString(): string`

Returns the string representation of the concept.

**Returns:** String representation of the underlying value

**Example:**
```typescript
const userId = new UserIdConcept('user-123');
console.log(userId.toString()); // "user-123"

const count = new OrderCountConcept(42);
console.log(count.toString()); // "42"
```

## Best Practices

### 1. Use for Domain Identifiers

Create ConceptAs types for all domain identifiers with the union type export pattern:

```typescript
// UserId.ts
class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

// ProductId.ts
class ProductIdConcept extends ConceptAs<string> {}
export type ProductId = ProductIdConcept | string;

// OrderId.ts
class OrderIdConcept extends ConceptAs<string> {}
export type OrderId = OrderIdConcept | string;

// CustomerId.ts
class CustomerIdConcept extends ConceptAs<string> {}
export type CustomerId = CustomerIdConcept | string;
```

### 2. Use for Measured Values

Wrap numeric values that represent specific measurements or counts:

```typescript
// OrderCount.ts
class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;

// Price.ts
class PriceConcept extends ConceptAs<number> {}
export type Price = PriceConcept | number;

// Quantity.ts
class QuantityConcept extends ConceptAs<number> {}
export type Quantity = QuantityConcept | number;

// TemperatureInCelsius.ts
class TemperatureInCelsiusConcept extends ConceptAs<number> {}
export type TemperatureInCelsius = TemperatureInCelsiusConcept | number;
```

### 3. Use Union Types for Flexibility

Export union types from your concept files to allow both ConceptAs instances and primitives, providing flexibility while maintaining proper serialization:

```typescript
// UserId.ts
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;
```

```typescript
// Order.ts
import { field } from '@cratis/fundamentals';
import { UserId, UserIdConcept } from './UserId';
import { OrderCount, OrderCountConcept } from './OrderCount';

class Order {
    // Use the exported union type in the type annotation
    // Use the concept class in the @field decorator
    @field(UserIdConcept)
    userId!: UserId;
    
    @field(OrderCountConcept)
    orderCount!: OrderCount;
}

// Both work correctly
order.userId = new UserIdConcept('user-123');
order.userId = 'user-456';
```

See the [Union Types for Flexible Assignment](#union-types-for-flexible-assignment) section for complete details.

### 4. Don't Add Extra Properties

> ⚠️ **Important**: ConceptAs types are meant to wrap a single primitive value only. Don't add additional properties to ConceptAs subclasses, as the JsonSerializer assumes they only contain the `value` property.

```typescript
// ❌ DON'T do this
class UserIdConcept extends ConceptAs<string> {
    createdAt: Date; // This will break serialization
}

// ✅ DO this instead - create a separate class
class User {
    @field(UserIdConcept)
    id!: UserId;
    
    @field(Date)
    createdAt!: Date;
}
```

### 5. Keep Concepts Simple

ConceptAs is for wrapping primitives. For complex domain objects with multiple properties, create regular classes instead.

## Comparison with C# ConceptAs

The TypeScript implementation mirrors the C# version but with JavaScript idioms:

| Feature | C# | TypeScript |
|---------|-----|-----------|
| Base class | `ConceptAs<T>` record | `ConceptAs<T>` abstract class |
| Value property | `Value` property | `value` readonly property |
| Implicit conversion | `implicit operator T` | `valueOf()` method |
| Flexible assignment | Implicit operators | Union types (e.g., `UserId | string`) |
| String representation | `ToString()` override | `toString()` method |
| JSON serialization | `ConceptAsJsonConverter` | Built into `JsonSerializer` |
| Serialization pattern | Unwrap → Serialize recursively | Unwrap → Serialize recursively |
| Comparison operators | `IComparable` interface | Not implemented (use `value` directly) |

**Key Difference**: TypeScript union types (`UserId | string`) provide a more explicit and type-safe way to allow both ConceptAs instances and primitives compared to C#'s implicit operators.

## See Also

- [JsonSerializer](./serialization/json_serializer.md) - JSON serialization with ConceptAs support
- [Field Decorator](./serialization/field_decorator.md) - Declaring fields for serialization
- [C# Concepts](../csharp/concepts.md) - C# equivalent documentation
