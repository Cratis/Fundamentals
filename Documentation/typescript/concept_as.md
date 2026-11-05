---
title: Model domain values with TypeScript ConceptAs
description: Use ConceptAs in Cratis Fundamentals to name TypeScript domain values, accept primitives with unions, and restore wrapper instances from JSON.
---
<a id="conceptas"></a>

`ConceptAs<T>` in Cratis Fundamentals wraps a value in a named TypeScript class, so a user ID or an order count has a domain name in your code. Use it when that name makes a field or function signature easier to understand. The wrapper stores the value; validation remains your responsibility.

Start by extending `ConceptAs` with the value's type. If callers should also be able to pass a plain string or number, export a union type: a type that accepts either the wrapper or its underlying value.

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;
```

<a id="overview"></a>

## Choose what the name should express

`UserId` tells a reader what a string means, while `OrderCount` gives a number a purpose. Neither name checks the value's contents. A union such as `UserIdConcept | string` accepts any string, so choose it when accepting raw values is useful to your callers.

The sections below use separate examples and labeled excerpts. For reading and writing JSON, go to [Using with JsonSerializer](#using-with-jsonserializer); for extracting a wrapped value, see the [API reference](#api-reference).

<a id="key-benefits"></a>

## What ConceptAs changes

<a id="1-type-safety"></a>

### Understand the type boundary

TypeScript compares the structure of instances, not just their class names. Two empty subclasses of `ConceptAs<string>` therefore remain interchangeable. In this example, a user ID is accepted as an order ID:

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderIdConcept extends ConceptAs<string> {}
export type OrderId = OrderIdConcept | string;

const acceptedAsOrder: OrderId = new UserIdConcept('user-123');
console.log(acceptedAsOrder.toString()); // "user-123"
```

If your API must reject a user ID where an order ID is expected, these empty subclasses do not provide that boundary. Validate formats and business rules before constructing a wrapper, too: the constructor stores whatever value you pass.

<a id="2-self-documenting-code"></a>

### Name values in function signatures

Domain names help explain a function even when they do not enforce distinct types. In the following before-and-after declaration excerpt, `CustomerId` and `InvoiceAmount` explain the arguments without requiring a reader to inspect the implementation. The excerpt uses the earlier `ConceptAs` import; it declares signatures rather than implementing invoice creation.

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

<a id="3-json-serialization-support"></a>

### Keep JSON values simple

`JsonSerializer.serialize()` writes the value inside a concept, so the JSON contains `"user-123"` rather than an object with a `value` property. When reading that JSON back, `@field(UserIdConcept)` tells the serializer which class to construct. The union type exists only for TypeScript's type checking and cannot supply that runtime constructor.

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

### Creating a ConceptAs type

Extend the `ConceptAs<T>` abstract class with the type you want to wrap. Strings suit identifiers, numbers suit counts or amounts, and booleans can express a named flag. The unions below let callers use either a wrapper instance or the corresponding primitive:

```typescript
import { ConceptAs } from '@cratis/fundamentals';

class UserIdConcept extends ConceptAs<string> {}
export type UserId = UserIdConcept | string;

class OrderCountConcept extends ConceptAs<number> {}
export type OrderCount = OrderCountConcept | number;

class IsActiveConcept extends ConceptAs<boolean> {}
export type IsActive = IsActiveConcept | boolean;
```

Use the exported union in a type annotation when both representations are acceptable. Keep the class available for operations that need a runtime value, such as construction, `instanceof`, or `@field` metadata.

### Using ConceptAs types

With the three declarations above, pass a value to the constructor to create a wrapper. Read it through `value` or `valueOf()`, and use `toString()` when you need text. Assigning a primitive to a union-typed variable leaves it as a primitive; that assignment does not construct a wrapper.

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

To restore concept instances on a model, mark each field with `@field` and pass its concept class. In the `Order` below, `@field(OrderIdConcept)` gives the serializer the constructor for `orderId`. The `OrderId` union annotation separately allows application code to assign either a wrapper or a string.

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

Both `instanceof` checks are true after deserialization, including the customer ID that began as a plain string. The JSON does not record which representation you assigned; the class passed to `@field` determines what the serializer reconstructs.

Wrappers are still objects. When comparing identifiers by their contents, compare their underlying values. A union annotation does not add value equality or make structurally identical concept classes incompatible.

### Union types for flexible assignment

A union is useful at an API boundary where callers may already have a raw value. Export the class alongside it so other modules can construct wrappers, test `instanceof`, and declare field metadata. Callers import the union for type annotations and the class for those runtime operations.

#### Recommended export pattern

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

The two exports serve different jobs: `UserId` names the accepted inputs, and `UserIdConcept` provides the constructor. Keeping both in the same module lets a model use that domain name while still supplying the serializer with a class.

#### Using the exported types

Import both exports when declaring a field that accepts primitives but must be reconstructed as a wrapper. Here, `userId` and `orderCount` use union annotations, while their decorators name the classes to create when reading JSON:

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

The following excerpts use this `Order` declaration and its imports. Each serialization example is a separate use of the model.

#### Assignment flexibility

The union accepts wrappers and primitives on the same field. These assignments change what the field holds without converting one representation into the other:

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

#### Serialization behavior

For the string and number concepts below, a field produces the same JSON shape whether it holds a wrapper or a primitive. The first order uses wrappers; the second uses raw values:

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

When reading JSON, present, non-null values become instances of the classes supplied to `@field`:

```typescript
const json = '{"userId":"user-123","orderCount":42}';
const order = JsonSerializer.deserialize(Order, json);

console.log(order.userId instanceof UserIdConcept); // ✅ true
console.log(order.orderCount instanceof OrderCountConcept); // ✅ true
console.log((order.userId as UserIdConcept).value); // "user-123"
console.log((order.orderCount as OrderCountConcept).value); // 42
```

The serializer reconstructs these wrappers from the field metadata, regardless of how the values were assigned before serialization. An omitted concept field keeps its constructor default; an explicit `null` remains `null`. See [JsonSerializer's field handling](./serialization/json_serializer.md#integration-with-field-decorators) when your payload can omit values.

#### Implementation details

`JsonSerializer` recognizes a concept instance, reads its `value`, and sends that value through serialization again. For a string, number, or boolean, the result is a JSON primitive. An inner object goes through its own serialization logic. The union annotation plays no part in this process because TypeScript types are absent at runtime.

#### Best practices for union types

Keep the runtime class exported when consumers need more than assignment. This module gives a service access to both the accepted input type and the constructor:

```typescript
// UserId.ts
import { ConceptAs } from '@cratis/fundamentals';

export class UserIdConcept extends ConceptAs<string> {}

export type UserId = UserIdConcept | string;
```

Using the `Order` declaration from [Using the Exported Types](#using-the-exported-types), this service accepts either representation and places it on the model. It leaves conversion to the serializer when the model is read back from JSON:

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

When an operation needs to distinguish a wrapper from a primitive, use an `instanceof` check. TypeScript then narrows the union so the wrapper branch can read `value`:

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

A union annotation alone cannot tell `JsonSerializer` to construct a concept. The alternative declarations below show the difference: use the decorated field when the JSON value needs to become a wrapper.

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

## API reference

### Constructor

```typescript
constructor(readonly value: T)
```

Stores the supplied value without validating or converting it. The example uses the earlier `ConceptAs` import.

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

Returns the wrapped value. JavaScript can call `valueOf()` during coercion, but TypeScript does not treat the wrapper itself as a number for arithmetic. Extract the number explicitly, as in `count.valueOf() + 10`:

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

## Best practices

<a id="1-use-for-domain-identifiers"></a>

### Name domain identifiers

Use a wrapper where its name helps explain an identifier's purpose in your application. These declaration excerpts use the earlier `ConceptAs` import. When placing them in separate modules, follow the [export pattern](#recommended-export-pattern) so callers can also import the classes.

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

<a id="2-use-for-measured-values"></a>

### Make numeric meaning visible

`TemperatureInCelsius` tells a reader which unit a number represents; `Quantity` tells them what it measures. These declarations use the same import and export setup as the identifier examples. Validate ranges and perform unit conversions separately.

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

<a id="3-use-union-types-for-flexibility"></a>

### Accept primitives where callers need them

Export a union when the field should accept both wrappers and primitives. Keep the class exported for field metadata, as this `Order.ts` continuation shows. It also imports `OrderCount.ts` from the [export pattern](#recommended-export-pattern).

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

<a id="4-dont-add-extra-properties"></a>

### Keep extra state on the containing model

A timestamp placed on a concept subclass disappears when `JsonSerializer` unwraps its `value`. Put that timestamp on the containing model and decorate it there if it needs to survive serialization. These contrasting excerpts use the earlier `ConceptAs`, `field`, and `UserId` declarations.

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

<a id="5-keep-concepts-simple"></a>

### Use regular classes for multi-property objects

Use `ConceptAs` when a single wrapped value expresses the domain meaning you need. If the object has several properties that must be stored together, make it a regular class and declare its serialized fields with `@field`.

## Comparison with C# ConceptAs

If you already use C# concepts, keep the language differences in view. The TypeScript wrapper shares the idea of naming an underlying value, but it does not provide C# record equality or make empty subclasses distinct types.

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

<a id="see-also"></a>

## Next steps

To restore concepts as part of a larger model, continue with [JsonSerializer](./serialization/json_serializer.md). The [field decorator reference](./serialization/field_decorator.md) covers the metadata you need for collections and other field types.

For the .NET side of your model, read [C# Concepts](../csharp/concepts.md) for its conversion and equality behavior rather than assuming the TypeScript rules apply.
