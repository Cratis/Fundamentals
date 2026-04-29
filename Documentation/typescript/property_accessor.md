# PropertyAccessor

The `PropertyAccessor<T>` type represents a function that accesses a property from an instance.

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

const value = getName({ id: '1', name: 'Ada' });
```

## Common Usage

`PropertyAccessor` is used with utilities that resolve property paths and metadata from lambda-style accessors.
