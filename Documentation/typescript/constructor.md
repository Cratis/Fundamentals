---
description: Use the Constructor type to accept a class constructor in TypeScript APIs that create or deserialize typed instances.
---
# Constructor

`Constructor<T>` describes a class constructor that produces an instance of `T`. Use it when a function needs the class itself at runtime, rather than an already-created instance or a TypeScript-only interface.

```typescript
export type Constructor<T = any> = new (...args: any[]) => T;
```

The generic parameter describes the instance returned by `new`, not the class's static members. The alias accepts any constructor arguments and defaults its result type to `any`, so specify `T` when callers should retain a known instance type.

A class value such as `Customer` can be passed as the constructor. An interface cannot: TypeScript erases interfaces, so they supply no runtime value to instantiate.

## Example

```typescript
import { Constructor } from '@cratis/fundamentals';

function createInstance<T>(ctor: Constructor<T>): T {
    return new ctor();
}

class Customer {
    name = 'Sample customer';
}

const customer = createInstance(Customer);
```

## Common Usage

The factory above calls `new ctor()` without arguments. Although the alias allows classes with required parameters, this factory does not supply them. Use classes that can be constructed without arguments for this example, or design your factory to pass the arguments its classes need. The broad alias alone does not enforce that restriction.

Fundamentals uses `Constructor` in field metadata and `JsonSerializer` entry points. For example, deserialization receives a target class and creates an instance with `new targetType()`. Passing a constructor tells the serializer which class to create; field metadata describes how to populate its properties. The alias itself performs neither serialization nor runtime validation.

## See also

- [PropertyAccessor](property_accessor.md) — accept a callback over an existing instance
- [JsonSerializer](serialization/json_serializer.md) — deserialize into a target class
- [Field decorator](serialization/field_decorator.md) — declare runtime field metadata
