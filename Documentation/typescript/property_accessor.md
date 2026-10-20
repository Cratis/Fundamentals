---
description: Use the PropertyAccessor type to pass strongly-typed property-selection callbacks in TypeScript.
---
# PropertyAccessor

`PropertyAccessor<T>` describes a callback that reads a value from an instance of `T`. Use it when an API accepts a property-selection function instead of a property name string.

The generic parameter checks the callback's input: with `PropertyAccessor<Customer>`, TypeScript knows which properties a customer has. It does not constrain the result to a particular property type, because the return type is `any`. Specify `T` rather than relying on the default `object` when you need member completion and property-name checking.

```typescript
export type PropertyAccessor<T = object> = (instance: T) => any;
```

Use this type when you need strongly-typed property-selection callbacks.

## Example

```typescript
import { PropertyAccessor } from '@cratis/fundamentals';

type Customer = {
    id: string;
    name: string;
};

const getName: PropertyAccessor<Customer> = customer => customer.name;

const value = getName({ id: '1', name: 'Sample customer' });
```

## Common Usage

This is a type alias, not a runtime wrapper. Calling the accessor executes your function against the instance you supply; it does not create an instance, check incoming data, or handle missing nested objects. For an optional nested value, put the required guard or optional chaining in the callback itself.

`PropertyAccessorDescriptor` pairs an accessor with an explicit array of path segments. Its `path` property joins those segments with dots; the descriptor does not infer them from the callback. Keep the segments consistent with the property your callback reads when constructing a descriptor.

For an API that must retain the selected value's type, use a more specific callback signature such as `(instance: Customer) => string` rather than expecting this alias to preserve that result type.

## See also

- [Constructor](constructor.md) — accept a runtime class rather than an instance callback
- [Field decorator](serialization/field_decorator.md) — describe runtime field types for serialization
