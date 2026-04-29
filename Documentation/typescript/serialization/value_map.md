# ValueMap Serialization

This page describes how `ValueMap<TKey, TValue>` is serialized and deserialized with `JsonSerializer`.

For general `ValueMap` behavior and API, see [ValueMap](../value_map.md).

## Why This Matters

When dictionaries with complex keys are serialized from the backend, key objects are represented as JSON property names. During frontend deserialization, you often create new key instances for lookups, which do not match by reference.

`ValueMap` with `JsonSerializer` and `@field` metadata ensures these entries are reconstructed as typed keys and values.

## Field Metadata and Generic Arguments

To deserialize a `ValueMap` with typed keys and typed values, provide generic arguments through the `@field` decorator options:

```typescript
import { field, ValueMap } from '@cratis/fundamentals';

class AccountKey {
    @field(String)
    tenant!: string;
}

class AccountSummary {
    @field(Number)
    balance!: number;
}

class Projection {
    @field(ValueMap, { genericArguments: [AccountKey, AccountSummary] })
    accounts!: ValueMap<AccountKey, AccountSummary>;
}
```

## Behavior

- Keys are deserialized using the first generic argument.
- Values are deserialized using the second generic argument.
- Lookup with a new key instance works when key content is equal.

## Example

```typescript
import { JsonSerializer } from '@cratis/fundamentals';

const projection = JsonSerializer.deserialize(Projection, json);

const key = new AccountKey();
key.tenant = 'tenant-a';

const summary = projection.accounts.get(key);

projection.accounts.set(key, { balance: 1200 });

for (const [accountKey, accountSummary] of projection.accounts.entries()) {
    console.log(accountKey.tenant, accountSummary.balance);
}
```

## Backend Interop

For backend serialization details, see [Complex Key Dictionaries](../../csharp/serialization/complex_key_dictionaries.md).
