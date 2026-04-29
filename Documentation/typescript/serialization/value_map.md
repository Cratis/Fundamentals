# ValueMap

`ValueMap<TKey, TValue>` is a map implementation for complex keys where key equality is based on value content instead of object reference identity.

## Why ValueMap

When dictionaries with complex keys are serialized from the backend, key objects are represented as JSON property names. During frontend deserialization you often create new key instances for lookups, which do not match by reference.

`ValueMap` solves this by comparing object keys by value.

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

- Map keys are deserialized using the first generic argument.
- Map values are deserialized using the second generic argument.
- Lookup with a new key instance works when key content is equal.

## Example

```typescript
const projection = JsonSerializer.deserialize(Projection, json);

const key = new AccountKey();
key.tenant = 'tenant-a';

const summary = projection.accounts.get(key);
```

## Backend Interop

For backend serialization details, see [Complex Key Dictionaries](../../csharp/serialization/complex_key_dictionaries.md).
