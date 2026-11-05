---
title: ValueMap
description: ValueMap in Cratis Fundamentals stores TypeScript entries with object keys matched by JSON representation rather than object reference.
---

A lookup key reconstructed from a response is a new object, even when it describes the same account. `ValueMap<TKey, TValue>` lets `get()` find an entry with that reconstructed key when its JSON representation matches the stored key. It is not a general deep-equality map.

## Why ValueMap

`Map` uses reference identity for object keys. This means two objects with identical property values are still treated as different keys.

`ValueMap` first accepts identical keys with `===`. For two different object keys, it compares their `JSON.stringify()` results. This suits small, stable keys whose property order and JSON representation you control.

## Public API

`ValueMap` has three public methods:

- `set(key, value)`: Adds a new entry or updates an existing entry for the key and returns the same `ValueMap` instance.
- `get(key)`: Returns the value for a matching key, or `undefined` if no match exists.
- `entries()`: Returns an iterator of `[key, value]` pairs in insertion order.

The JSON comparison has consequences for the keys you choose. Property order can change the serialized text, and properties omitted by JSON, such as an `undefined` property, can make different objects match. Circular objects can throw when comparison reaches `JSON.stringify()`; looking up the identical object can instead match at the identity check.

Keys are stored as references, not copied or frozen. Mutating an inserted object changes what future reconstructed keys must match. Keep keys stable after insertion; if reference identity is the contract you want, use a native `Map`.

## Example

```typescript
import { ValueMap } from '@cratis/fundamentals';

type Key = { tenant: string; account: string };

type Summary = { balance: number };

const accounts = new ValueMap<Key, Summary>();

accounts.set({ tenant: 'tenant-a', account: 'savings' }, { balance: 1200 });

const lookup = { tenant: 'tenant-a', account: 'savings' };
const summary = accounts.get(lookup);
console.log(summary?.balance); // 1200

for (const [key, value] of accounts.entries()) {
    console.log(key.tenant, key.account, value.balance);
}
```

The lookup uses a different object with the same property order and values, so it retrieves the stored balance of 1200. A key with no match makes `get()` return `undefined`; handle that case before using the summary.

## Serialization-Specific Usage

When `ValueMap` is used with `JsonSerializer` and `@field` metadata, use the serialization guidance in [ValueMap Serialization](./serialization/value_map.md).
