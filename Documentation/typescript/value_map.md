# ValueMap

`ValueMap<TKey, TValue>` is a map implementation for complex keys where key equality is based on value content instead of object reference identity.

## Why ValueMap

`Map` uses reference identity for object keys. This means two objects with identical property values are still treated as different keys.

`ValueMap` solves this by comparing object keys by value, which is useful when key instances are reconstructed across boundaries.

## Public API

`ValueMap` has three public methods:

- `set(key, value)`: Adds a new entry or updates an existing entry for the key and returns the same `ValueMap` instance.
- `get(key)`: Returns the value for a matching key, or `undefined` if no match exists.
- `entries()`: Returns an iterator of `[key, value]` pairs in insertion order.

For object keys, matching uses value equality (serialized JSON content) rather than reference identity.

## Example

```typescript
import { ValueMap } from '@cratis/fundamentals';

type Key = { tenant: string; account: string };

type Summary = { balance: number };

const accounts = new ValueMap<Key, Summary>();

accounts.set({ tenant: 'tenant-a', account: 'savings' }, { balance: 1200 });

const lookup = { tenant: 'tenant-a', account: 'savings' };
const summary = accounts.get(lookup);

for (const [key, value] of accounts.entries()) {
    console.log(key.tenant, key.account, value.balance);
}
```

## Serialization-Specific Usage

When `ValueMap` is used with `JsonSerializer` and `@field` metadata, use the serialization guidance in [ValueMap Serialization](./serialization/value_map.md).
