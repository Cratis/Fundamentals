---
description: Create strongly-typed TypeScript identifiers and value objects that wrap string, number or boolean with the ConceptAs class.
---
# ConceptAs

When a string represents a user ID rather than arbitrary text, `ConceptAs<string>` gives that value a named wrapper. Read its primitive with `value` or `valueOf()`, and use the wrapper class with `@field` when `JsonSerializer` needs to rebuild it from JSON.

If an API should also accept the primitive directly, export a union of the class and primitive type:

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;
```

## Overview

Concept names express what a value means in your code. They do not validate its contents or make empty subclasses nominally distinct: TypeScript checks their instance structure. A union such as `UserIdConcept | string` also accepts any string. Choose that convenience deliberately, rather than relying on it to prevent identifier mix-ups.

The examples below are separate examples or labeled excerpts, not one file to concatenate. Start with the wrapper, then use [Using with JsonSerializer](#using-with-jsonserializer) when you need a JSON response restored as a class instance.

## Key Benefits

### 1. Type Safety

Two empty subclasses wrapping strings have the same public instance structure. The assignment below is accepted; the different class names do not create a compile-time boundary:

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderIdConcept extends ConceptAs<string> {}
export type OrderId = OrderIdConcept | string;

const acceptedAsOrder: OrderId = new UserIdConcept('user-123');
console.log(acceptedAsOrder.toString()); // "user-123"
```

If an input must satisfy a format or business rule, validate it before wrapping it. Neither the constructor nor the union supplies that validation.

### 2. Self-Documenting Code

The type names still help a reader understand a signature. This declaration excerpt assumes the `ConceptAs` import above; it describes the before/after signatures, not an executable invoice operation:

```typescript
// Before: What does this string represent?
declare function createInvoice(customerId: string, amount: number): void;

// After: Clear semantic meaning
class CustomerIdConcept extends ConceptAs<string> {}
export type CustomerId = CustomerIdConcept | string;

class InvoiceAmountConcept extends ConceptAs<number> {}
export type InvoiceAmount = InvoiceAmountConcept | number;

declare function createInvoice(customerId: CustomerId, amount: InvoiceAmount): void;
```

### 3. JSON Serialization Support

`JsonSerializer.serialize()` unwraps a concept to its inner value. To rebuild a concept on a model field, give `@field` the concept class; a union type alone supplies no runtime constructor. The instance check below shows what that metadata restores:

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
- **Runtime reconstruction**: `@field` supplies the class to construct during deserialization, independently of the union annotation

### Using ConceptAs Types

Continuing from the three declarations above, pass the underlying value to the constructor. `valueOf()` returns that value for an operation; the union also lets you assign primitives without constructing a wrapper:

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

After deserialization, both decorated IDs are concept instances even though one was assigned as a string before serialization. The class passed to `@field` controls that result; the union controls which assignments TypeScript accepts.

A TypeScript union such as `UserId | string` explicitly permits both representations. It does not make two structurally identical concepts incompatible or supply value equality; compare the underlying values when that is the operation you need.

### Union Types for Flexible Assignment

Use a union when accepting either representation is useful to your callers. Export the class as well when another module needs to construct it, test `instanceof`, or pass it to `@field`. The union is a type annotation, not a runtime value or a validation boundary.

#### Recommended Export Pattern

These two modules export both the runtime class and the union type:

```typescript
// UserId.ts
import { ConceptAs } from '@cratis/fundamentals';

export class UserIdConcept extends ConceptAs<string> {}

export type UserId = UserIdConcept | string;
```

```typescript
// OrderCount.ts
import { ConceptAs } from '@cratis/fundamentals';

export class OrderCountConcept extends ConceptAs<number> {}

export type OrderCount = OrderCountConcept | number;
```

**Benefits of this pattern:**

- **Convenience**: Consumers can assign either the concept instance or the primitive value
- **Readability**: The exported type name (`UserId`) is clean and matches domain language
- **Runtime access**: Consumers import `UserIdConcept` when they need the constructor or field metadata
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

The following assignment and serialization excerpts use this `Order` declaration and its imported concept classes. Treat each serialization alternative as a separate use of that model, not another declaration in the same scope.

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

For a string or number concept, serialization writes the primitive whether the field holds a wrapper or the primitive itself. Deserialization uses the declared field class to wrap the incoming value:

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

**Deserialization** - Present, non-null field values become concept instances based on the `@field` decorator:

```typescript
const json = '{"userId":"user-123","orderCount":42}';
const order = JsonSerializer.deserialize(Order, json);

console.log(order.userId instanceof UserIdConcept); // ✅ true
console.log(order.orderCount instanceof OrderCountConcept); // ✅ true
console.log((order.userId as UserIdConcept).value); // "user-123"
console.log((order.orderCount as OrderCountConcept).value); // 42
```

For these non-null inputs, the class specified in `@field` is restored regardless of whether the field originally held a wrapper or a primitive. An omitted field keeps its constructor default; an explicit `null` remains `null`, as described in [JsonSerializer](./serialization/json_serializer.md#integration-with-field-decorators).

#### Implementation Details

When serializing a concept, `JsonSerializer` performs these operations:

1. **Recognition**: During serialization, the serializer detects ConceptAs instances
2. **Unwrapping**: Extracts the inner value from the ConceptAs instance
3. **Recursive Serialization**: Calls the serializer recursively on the inner value to handle complex types properly

This ensures that:

- Simple primitives (string, number, boolean) are serialized directly
- Complex inner types are properly serialized using their own serialization logic
- Union annotations need no runtime handling; `@field` supplies the class for reconstruction

#### Best Practices for Union Types

Export the class alongside the union if consumers need runtime operations:

```typescript
// UserId.ts
import { ConceptAs } from '@cratis/fundamentals';

export class UserIdConcept extends ConceptAs<string> {}

export type UserId = UserIdConcept | string;
```

This continuation excerpt uses the `Order` declaration from [Using the Exported Types](#using-the-exported-types). Its method accepts either representation and returns that model:

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

const service = new OrderService();

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

When a field needs reconstruction as a concept, supply `@field`. The next excerpt contrasts two alternative `Order` declarations using the types above; choose one, rather than placing both declarations in a file:

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

The underlying value. `readonly` prevents reassignment through TypeScript; it does not freeze the instance at runtime or deeply freeze an object held as the value.

**Example:**

```typescript
const userId = new UserIdConcept('user-123');
console.log(userId.value); // "user-123"
```

### Methods

#### `valueOf(): T`

Returns the primitive value of the concept. JavaScript can call it during coercion, but that does not make arithmetic on a wrapper a valid TypeScript expression. Call `valueOf()` explicitly before numeric operations:

**Returns:** The underlying value

**Example:**

```typescript
class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;

const count = new OrderCountConcept(42);
console.log(count.valueOf()); // 42
console.log(count.valueOf() + 10); // 52 (explicit primitive extraction)
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

Use a named wrapper where it helps distinguish an identifier's purpose. These declaration excerpts assume the earlier `ConceptAs` import; for separate modules, follow [Recommended Export Pattern](#recommended-export-pattern) to export the classes as well as their union types:

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

A wrapper can also make the unit or meaning of a number visible. These declaration excerpts use the same import and export setup as the identifier examples; they do not add unit conversion or range validation:

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

For a field that should accept either representation, export its union and runtime class. The `Order.ts` continuation also uses `OrderCount.ts` from [Recommended Export Pattern](#recommended-export-pattern):

```typescript
// UserId.ts
import { ConceptAs } from '@cratis/fundamentals';

export class UserIdConcept extends ConceptAs<string> {}
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

const order = new Order();

// Both work correctly
order.userId = new UserIdConcept('user-123');
order.userId = 'user-456';
```

See the [Union Types for Flexible Assignment](#union-types-for-flexible-assignment) section for complete details.

### 4. Don't Add Extra Properties

`JsonSerializer` unwraps the concept's `value`; additional subclass properties are not preserved. These contrasting excerpts use the earlier `ConceptAs`, `field` and `UserId` declarations. Keep a timestamp on the containing model when it needs to survive serialization:

```typescript
// ❌ DON'T do this
class UserIdConcept extends ConceptAs<string> {
    createdAt!: Date; // This extra state is lost when the concept is unwrapped
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

The names are related, but the TypeScript wrapper does not inherit C# record equality or nominal type distinctions. Read the language-specific contracts separately:

| Feature | C# | TypeScript |
|---------|----|------------|
| Base class | `ConceptAs<T>` record | `ConceptAs<T>` abstract class |
| Value property | `Value` property | `value` readonly property |
| Primitive extraction | `implicit operator T` | Call `valueOf()` explicitly in TypeScript expressions |
| Flexible assignment | Implicit operators | Union types (e.g., `UserId \| string`) |
| String representation | `ToString()` override | `toString()` method |
| JSON serialization | `ConceptAsJsonConverter` | Built into `JsonSerializer` |
| Serialization pattern | Unwrap → Serialize recursively | Unwrap → Serialize recursively |
| Comparison operators | `IComparable` interface | Not implemented (use `value` directly) |

## See Also

- [JsonSerializer](./serialization/json_serializer.md) - JSON serialization with ConceptAs support
- [Field Decorator](./serialization/field_decorator.md) - Declaring fields for serialization
- [C# Concepts](../csharp/concepts.md) - C# equivalent documentation
