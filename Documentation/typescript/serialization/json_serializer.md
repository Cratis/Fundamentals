---
title: TypeScript JsonSerializer for typed JSON models
description: JsonSerializer reads JSON into TypeScript class instances using field metadata and converters, so model methods and nested types are available.
---
<a id="jsonserializer"></a>

`JsonSerializer` reads JSON into TypeScript class instances using runtime field metadata. Use it when `JSON.parse()` gives you plain objects but your code needs model methods, nested class instances, or values such as `Date` and `Guid`. It also writes those models back to JSON.

## Overview

Pass the model's constructor to `deserialize` and declare its readable fields with `@field`. Cratis Fundamentals uses that metadata to reconstruct field values, calling built-in or registered converters where needed. For fields with several possible implementations, `@derivedType` identifiers let the reader choose a candidate class.

This is reconstruction, not schema validation. Primitive field readers pass values through without checking their types, so validate untrusted input separately.

## Key Benefits

<a id="1-type-safe-runtime-objects"></a>

### 1. Construct Class Instances

A TypeScript annotation does not turn a parsed object into a class instance. `JsonSerializer.deserialize(User, json)` constructs `User` and populates its declared fields. These excerpts use the decorated `User` class shown in the next section.

```typescript
import { JsonSerializer } from '@cratis/fundamentals';

// Traditional approach - loses type information
const json = `{"name":"John","age":30}`;
const plainUser = JSON.parse(json); // Plain object, not a User

// With JsonSerializer - maintains type information
const user = JsonSerializer.deserialize(User, json); // Real User instance
console.log(user instanceof User); // ✅ true
```

### 2. Business Logic Access

Because the result is a `User` instance, its prototype provides `getDisplayName()`. JSON supplies only the data; the method comes from your class definition. Import `field` alongside `JsonSerializer` for this model.

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

A polymorphic field can select its implementation from `_derivedTypeId`. This excerpt shows identifier registration only; the [derived types guide](./derived_types.md) supplies the interface and decorated payment fields omitted from these class bodies. It assumes a containing `Payment` class whose `method` field declares `CreditCard` and `PayPal` as candidates.

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

The decorator registers subclasses against their base classes when their modules are evaluated. Import those modules before reading payloads. For an interface-shaped field, list the implementations on `@field`, or register them with an explicit `targetType` constructor and use that constructor for the field.

If no candidate matches, the field's declared constructor is used and the incoming identifier is preserved. Root calls to `deserialize` and `deserializeArray` use the supplied constructor; they do not select a root subtype from `_derivedTypeId`.

### 4. Proper Type Conversion

Declare `Date` or `Guid` as the field's runtime type to invoke its built-in converter. In this model, `createdAt` and `customerId` arrive as JSON strings but are read as `Date` and `Guid` instances. Import `Guid` from `@cratis/fundamentals` for the excerpt.

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

Pass an instance to `serialize` to get a JSON string. Serialization visits runtime own properties, including properties without `@field`; the decorator controls typed reading, not which properties are written.

```typescript
const user = new User();
user.name = "John";
user.age = 30;

const json = JsonSerializer.serialize(user);
console.log(json); // {"name":"John","age":30}
```

### Basic Deserialization

Pass the constructor and a JSON string to `deserialize`. For models without a converter, the constructor is called without arguments, so it must support that call. The reader then populates the decorated fields.

```typescript
const json = `{"name":"John","age":30}`;
const user = JsonSerializer.deserialize(User, json);

console.log(user instanceof User); // ✅ true
console.log(user.name); // "John"
```

### Array Deserialization

Use `deserializeArray` when the JSON root is an array whose elements use the same runtime constructor.

```typescript
const json = `[
  {"name":"John","age":30},
  {"name":"Jane","age":25}
]`;

const users = JsonSerializer.deserializeArray(User, json);
// Array of User instances, not plain objects
```

## Integration with Field Decorators

Declare collections either as `@field(ItemType, true)` or as `@field(Array, { genericArguments: [ItemType] })`. Both forms read each element using its declared type, including concepts and nested models. An absent collection becomes `[]`; an explicit `null` stays `null`.

For non-collection fields, an omitted field or an `undefined` input value leaves the constructor's default untouched. This preserves initializers, including `null`, object references, and references to the instance itself. A field without an initializer stays `undefined`. A present value overrides the default after conversion; an explicit `null` overrides it without conversion. These rules also apply to fields inside nested decorated types.

For a concept whose underlying value needs conversion, declare its runtime type on the concept. For example, `class Identifier extends ConceptAs<Guid> { static readonly valueType = Guid; }` converts a JSON string into a `Guid` before wrapping it in `Identifier`. Concepts wrapping `""`, `0`, or `false` are also reconstructed; an omitted concept field keeps its constructor default.

Use the [field decorator guide](./field_decorator.md) to configure generic arguments, runtime metadata, and compiler decorator modes.

## Integration with Derived Types

When a field can hold several implementations, follow the [derived types guide](./derived_types.md) to give each class an identifier and make its constructor available to the field reader. A TypeScript interface alone cannot supply that runtime information.

## Best Practices

### 1. Complete Field Declaration

Decorate every field you need to reconstruct with a runtime type. An undecorated property can be written to JSON but will not be populated when reading a concrete model class.

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

Check both the data and the behavior you need after reading it back. This test excerpt uses the `User` model above and assumes your test framework provides `expect`.

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

Declare nested constructors just as you would primitive field types. This excerpt assumes decorated `Customer` and `Product` classes and an order payload in `json`. Reading `Order` also constructs its customer and each product in `items`.

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

Handle parse or converter errors where your application receives the payload, so it can report or reject unreadable data. A `try`/`catch` catches those failures; it does not add validation for values the reader accepts.

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

After receiving a response, reconstruct it with the model constructor. The backend's property names and value formats must match your field declarations and converters. The excerpt below parses the response and converts it back to a string; if you already have a parsed object, `JsonSerializer.deserializeFromInstance(User, json)` avoids that extra stringify/parse step.

```typescript
// Response from backend
const response = await fetch('/api/users/123');
const json = await response.json();

// Convert to typed instance
const user = JsonSerializer.deserialize(User, JSON.stringify(json));
```

### Working with Collections

For an array response, use `deserializeArray` with a JSON string, as below. If you already parsed the response, `deserializeArrayFromInstance(User, usersJson)` reads the array directly.

```typescript
const usersJson = await (await fetch('/api/users')).json();
const users = JsonSerializer.deserializeArray(User, JSON.stringify(usersJson));
```

### Polymorphic API Responses

Put a mixed collection on a model field so each element can resolve its identifier against that field's candidates. These excerpts assume configured `Payment`, `CreditCard`, and `PayPal` classes; replace the single-field payload's `"..."` identifier with a registered ID.

```typescript
// Single polymorphic field
const paymentJson = `{"method":{"amount":99.99,"_derivedTypeId":"..."}}`;
const payment = JsonSerializer.deserialize(Payment, paymentJson);

// Array of polymorphic types: declare the array as a field of a model.
// deserializeArray needs a runtime constructor, and an interface such as
// IPaymentMethod is erased at runtime, so it cannot be passed.
class PaymentMethods {
    @field(Object, true, [CreditCard, PayPal])
    methods!: IPaymentMethod[];
}

const methodsJson = `{"methods":[
  {"amount":50,"_derivedTypeId":"credit-card"},
  {"amount":40,"_derivedTypeId":"paypal"}
]}`;
const { methods } = JsonSerializer.deserialize(PaymentMethods, methodsJson);
```

See [Derived Types](./derived_types.md) for the field-based pattern.

## Custom converters

Built-in converters cover `Date`, `DateOnly`, `Guid`, `TimeOnly`, `TimeSpan`, `ValueMap`, and the [geospatial types](./geospatial.md). Register a custom converter when your payload needs a different representation.

Extend `JsonConverter`, return the constructor from `type`, and register an instance during application startup. This example replaces the `Date` converter. Because it reads local midnight but writes through UTC, check the intended timezone behavior before using this date-only representation.

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

Registration replaces the converter for that type, including a built-in converter. The reader calls `read` and the writer calls `write`, so replacing `Date` affects all date conversions through that serializer registry. To restore the built-in date representation, register a new `DateJsonConverter`:

```typescript
import { DateJsonConverter } from '@cratis/fundamentals';

JsonSerializer.registerConverter(new DateJsonConverter());
```

### What a converter cannot take over

Some values resolve before converter lookup:

- A `ConceptAs` type uses its underlying value's conversion before wrapping or unwrapping it. Register for the underlying type to affect that conversion.
- A `ValueMap` field is read from its declared generic arguments, not through a registered map converter. Writing a `ValueMap` does use the converter, so replacing it changes only the outbound half.
- A `Guid`, `DateOnly`, `TimeOnly`, or `TimeSpan` used as a `ValueMap` key uses its canonical string form, bypassing its registered converter.

`JsonSerializer` resolves converters by their `type` property. It does not call `JsonConverter.canConvert`, so overriding that method has no effect.

## Loaded more than once

Two copies of `@cratis/fundamentals` loaded into the same JavaScript realm have separate converter registries and class objects. Built-in keyed types, such as `Guid` and concepts, can still be recognized across copies, but that does not share the registries or make the constructors identical.

A converter registration reaches only its own copy. An `instanceof` check against the other copy's class is `false`, and updating the top-level dependency does not update a nested copy. These differences can change behavior without a build error.

The package reports duplicate loading once, when the second copy registers:

```text
[@cratis/fundamentals] Loaded 2 times into the same JavaScript realm, and it has to be loaded once.
```

The report names each load location. If they point to separate installs, inspect the dependency tree with `yarn why @cratis/fundamentals` and align incompatible version requirements before deduplicating with `yarn dedupe @cratis/fundamentals` or `npm dedupe`. Deduplication alone cannot combine conflicting version requirements.

If the paths differ only in `dist/esm` and `dist/cjs`, one install is being loaded as both ESM and CommonJS. Make importers resolve the same module format; deduplicating the dependency tree does not fix that case. Copies predating the duplicate-load guard do not register themselves, so the report cannot detect them.

### If you publish a package that uses this one

Declare `@cratis/fundamentals` as a peer dependency so the consuming application supplies the shared copy. Use a development dependency for your package's own build and specs; this excerpt shows that manifest structure.

```json
{
  "peerDependencies": { "@cratis/fundamentals": "^7" },
  "devDependencies":  { "@cratis/fundamentals": "7.16.8" }
}
```

A regular dependency allows a separate nested copy. Conflicting exact pins can force that duplication, because one installed version cannot satisfy both requirements. Choose a peer range that covers the versions your package supports, and check that the application's dependency tree and module resolution actually load one copy. A peer declaration alone does not prove that runtime condition.

## See Also

Configure the model's runtime types in the [field decorator guide](./field_decorator.md), or continue with [derived types](./derived_types.md) for mixed implementations. The [serialization overview](./index.md) connects both parts of the model setup.
