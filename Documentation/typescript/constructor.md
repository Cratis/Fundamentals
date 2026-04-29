# Constructor

The `Constructor<T>` type represents a class constructor signature.

```typescript
export type Constructor<T = any> = new (...args: any[]) => T;
```

Use this type when APIs need a runtime constructor function for creating or deserializing typed instances.

## Example

```typescript
import { Constructor } from '@cratis/fundamentals';

function createInstance<T>(ctor: Constructor<T>): T {
    return new ctor();
}

class Customer {
    name = 'Ada';
}

const customer = createInstance(Customer);
```

## Common Usage

In Fundamentals, `Constructor<T>` is commonly used by decorators and serialization helpers to keep runtime type information.
