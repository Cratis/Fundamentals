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
import { JsonSerializer } from '@cratis/fundamentals';

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

The runtime resolution flow is:

1. Read `_derivedTypeId` from the payload.
2. Build candidates from both `field.derivatives` and `DerivedType.getDerivedTypesFor(field.type)`.
3. Match candidate identifier and deserialize into the matching constructor.

This makes class inheritance work with runtime-only registration from `@derivedType`.
For interface-only polymorphism, include explicit derivatives on `@field` (or use `@derivedType`
with an explicit `targetType` constructor).

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

## Custom converters

`Date`, `Guid`, `TimeSpan`, `ValueMap` and the geospatial types come with converters built in. When a
type needs to cross the wire in a shape only you can decide — or when a built-in shape is not the one
your backend declared — register a converter for it.

Extend `JsonConverter`, declare the type it handles, and register it once during application start-up:

```typescript
import { JsonConverter, JsonSerializer, Constructor } from '@cratis/fundamentals';

class CalendarDateJsonConverter extends JsonConverter<Date> {
    get type(): Constructor<Date> {
        return Date;
    }

    read(value: string): Date {
        return new Date(`${value}T00:00:00`);
    }

    write(value: Date): string {
        return value.toISOString().substring(0, 10);
    }
}

JsonSerializer.registerConverter(new CalendarDateJsonConverter());
```

The converter is used for both directions — `read` on the way in, `write` on the way out — and takes
the place of any converter already registered for the same type, including a built-in one. Registering
only ever adds, so you can hand back to a built-in by registering a fresh one:

```typescript
import { DateJsonConverter } from '@cratis/fundamentals';

JsonSerializer.registerConverter(new DateJsonConverter());
```

### What a converter cannot take over

Two shapes resolve ahead of the converters, so a converter registered for them is not reached:

- A type deriving from `ConceptAs` is unwrapped to its underlying value in both directions and converted
  as that value's type. Register for the underlying type to reach a concept.
- A `ValueMap` is read back from the declaring field's generic arguments rather than through a
  converter. Note the asymmetry: writing a `ValueMap` *does* go through the registered converter, so
  replacing that one changes the outbound half only.

> [!NOTE]
> `JsonConverter` also defines `canConvert`. `JsonSerializer` does not call it — a converter is resolved
> by the type it declares through `type`, which is what `canConvert` would answer anyway — so overriding
> it has no effect.

## Loaded more than once

If two copies of `@cratis/fundamentals` end up in the same JavaScript realm, each builds its own
converter registry and its own `Guid` and `ConceptAs` class objects.

Values themselves survive that boundary — a convertible type is recognised by the key it declares
rather than by its class object, so a `Guid` or a concept created by one copy serialises correctly
through the other. Three things do not survive it, and all three are silent:

- A converter registered through `registerConverter` reaches only the copy it was registered on.
- `instanceof` against the other copy's class is `false`, including in your own code.
- A version pinned at the top level does not reach a copy nested under a dependency, so a fix you
  adopt can reach nothing at all while every build stays green.

So the package reports it once, on load:

```text
[@cratis/fundamentals] Loaded 2 times into the same JavaScript realm, and it has to be loaded once.
```

The report names where each copy was loaded from. Two separate installs collapse with
`yarn dedupe @cratis/fundamentals` (or `npm dedupe`); confirm with `yarn why @cratis/fundamentals`. If
the two paths differ only in `dist/esm` against `dist/cjs` it is one install reached as both ESM and
CommonJS, which no dedupe will fix — make every importer resolve the same one.

### If you publish a package that uses this one

Declare it as a **peer** dependency, not a regular one:

```json
{
  "peerDependencies": { "@cratis/fundamentals": "^7" },
  "devDependencies":  { "@cratis/fundamentals": "7.16.8" }
}
```

A regular dependency tells the package manager that a copy each is acceptable, and an exact pin makes
duplication unavoidable rather than merely possible — an application combining your package with any
other pinning a different `7.x` ends up with two physical copies that no dedupe can collapse. A peer
dependency is the mechanism for "exactly one of these in the tree", which is what this is. Keep the
range wide so a patch release does not force lockstep releases, and pin exactly in `devDependencies` so
your own build and specs stay where they were.

## See Also

- [Field Decorator](./field_decorator.md) - Decorator system for field serialization configuration
- [Derived Types](./derived_types.md) - Polymorphic type handling and interface implementations
- [Serialization Overview](./index.md) - Complete serialization system documentation
