# `@field` Decorator

The `@field` decorator supplies the runtime type information that `JsonSerializer` uses to serialize and deserialize class fields. The same decorator supports TypeScript's legacy decorator transform and the standard decorator transform.

## Signature

```typescript
field(
    targetType: Constructor,
    enumerable?: boolean,
    derivatives?: Constructor[],
    genericArguments?: Constructor[]
)

field(
    targetType: Constructor,
    options?: {
        enumerable?: boolean;
        derivatives?: Constructor[];
        genericArguments?: Constructor[];
    }
)
```

| Parameter | Description | Default |
|---|---|---|
| `targetType` | Runtime constructor for the field value, such as `String`, `Date`, or a custom class. | Required |
| `enumerable` | Indicates that the field contains a collection of `targetType` values. | `false` |
| `derivatives` | Constructors that may occur in a polymorphic field. | `[]` |
| `genericArguments` | Runtime constructors for generic type arguments. | `[]` |

The constructor argument is always explicit. `@field(String)`, for example, records `String` directly and does not depend on emitted `design:type` metadata.

## Supported Fields

Apply `@field` to public instance fields with string names.

The supported surface does not include:

- static fields;
- ECMAScript private fields declared with `#`;
- symbol-named fields;
- declarations that are not fields.

Methods do not need `@field`. Leave implementation-only state undecorated.

```typescript
import { field } from '@cratis/fundamentals';

export class Product {
    @field(String)
    name!: string;

    @field(Number)
    price!: number;

    calculateTax(rate: number): number {
        return this.price * rate;
    }
}
```

## Decorator Modes

The source syntax is identical in both modes. Your compiler configuration determines which decorator protocol calls `@field`.

### Legacy TypeScript Decorators

Set `experimentalDecorators` to `true` for the legacy TypeScript transform:

```json
{
  "compilerOptions": {
    "experimentalDecorators": true
  }
}
```

`@field` does not require `emitDecoratorMetadata`, because every use supplies its runtime constructor explicitly. Other libraries in your application may still require that legacy option.

### Standard TypeScript Decorators

Standard decorator metadata support requires TypeScript 5.2 or later. Omit `experimentalDecorators`, or set it to `false`, to use standard decorators:

```json
{
  "compilerOptions": {
    "experimentalDecorators": false,
    "emitDecoratorMetadata": false
  }
}
```

Standard decorators do not support `emitDecoratorMetadata`. `@field` does not need it.

With standard decorators, metadata is recorded while the class definition is evaluated and exposed through `Symbol.metadata`. Fundamentals defines `Symbol.metadata` on runtimes that do not provide it, so `Fields` and `JsonSerializer` can read field metadata before the first class instance is constructed. Importing `field` loads this support; you do not need a separate `reflect-metadata` import.

### Babel, Expo, and Hermes

If Babel compiles your decorated classes, configure `@babel/plugin-proposal-decorators` with `version: "2023-11"` for standard decorators. Expo applications normally receive decorator support through `babel-preset-expo`; keep the preset current and let Babel transform decorators before Hermes executes the output. Hermes should not be treated as a native decorator transform.

## Field Types

### Primitive and Custom Types

```typescript
import { field, Guid } from '@cratis/fundamentals';

export class Address {
    @field(String)
    city!: string;
}

export class Customer {
    @field(Guid)
    id!: Guid;

    @field(String)
    name!: string;

    @field(Date)
    registeredAt!: Date;

    @field(Address)
    address!: Address;
}
```

### Collections

Set `enumerable` to `true` for arrays:

```typescript
export class ShoppingCart {
    @field(String, true)
    tags!: string[];

    @field(Product, { enumerable: true })
    products!: Product[];
}
```

### Polymorphic Fields

Supply the possible runtime constructors for an interface-shaped or base-typed field:

```typescript
import { derivedType, field } from '@cratis/fundamentals';

export interface PaymentMethod {
    amount: number;
}

@derivedType('card')
export class CardPayment implements PaymentMethod {
    @field(Number)
    amount!: number;

    @field(String)
    lastFourDigits!: string;
}

@derivedType('invoice')
export class InvoicePayment implements PaymentMethod {
    @field(Number)
    amount!: number;

    @field(String)
    invoiceNumber!: string;
}

export class Payment {
    @field(Object, { derivatives: [CardPayment, InvoicePayment] })
    method!: PaymentMethod;

    @field(Object, {
        enumerable: true,
        derivatives: [CardPayment, InvoicePayment]
    })
    alternatives!: PaymentMethod[];
}
```

See [Derived Types](./derived_types.md) for derived type identifiers and runtime resolution.

## Inheritance

`Fields.getFieldsForType()` includes fields declared by base classes and derived classes. If a derived class redeclares a field with the same name, its declaration replaces the base declaration for that derived type without changing the base type's metadata.

This behavior is the same for legacy and standard decorators.

## Runtime Inspection

Use `Fields.getFieldsForType()` to inspect the metadata recorded for a class:

```typescript
import { Fields } from '@cratis/fundamentals';

const fields = Fields.getFieldsForType(Product);

for (const productField of fields) {
    console.log(productField.name);
    console.log(productField.type);
    console.log(productField.enumerable);
}
```

Each returned `Field` contains `name`, `type`, `enumerable`, `derivatives`, and `genericArguments`.

## See Also

- [JsonSerializer](./json_serializer.md) - Serialization and deserialization APIs
- [Derived Types](./derived_types.md) - Polymorphic type registration and resolution
