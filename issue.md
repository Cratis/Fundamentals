# Serialization of Dictionaries

In JavaScript/TypeScript we want to support a Map type that can work with complex value types.
We have records in the backend that gets generated as classes and we want to use these as keys
in the frontend in the same way we do for these in dictionaries in the backend.

## ValueMap

```typescript
export class ValueMap<K, V> {
  private entries: Array<[K, V]> = [];

  set(key: K, value: V): this {
    const index = this.entries.findIndex(([k]) => this.equals(k, key));
    if (index >= 0) {
      this.entries[index][1] = value;
    } else {
      this.entries.push([key, value]);
    }
    return this;
  }

  get(key: K): V | undefined {
    return this.entries.find(([k]) => this.equals(k, key))?.[1];
  }

  private equals(a: K, b: K): boolean {
    if (a instanceof EventType && b instanceof EventType) {
      return a.id === b.id && a.generation === b.generation && a.tombstone === b.tombstone;
    }
    return a === b;
  }
}
```

## Field Decorator

The `@field()` decorator needs a `genericArguments` optional property that takes an
array of constructors. This is needed to be able to deserialize keys into their correct type.

## Backend Serialization

For this to work, we need to have a general `IDictionary<,>` that recognizes if its a complex type
(we don't want to handle primitive types like string) and then do the correct serialization and
deserialization. It should maintain the JsonSerializerOptions as given, but disable the writing of indented
JSON for the key value.

I've done a spike for this here that can work as a starting point:
/Volumes/Code/Playground/Serialization/ComplexKeyDictionaryConverterFactory.cs

Add the serializer to the `Globals.cs` setup.

## Frontend Serialization

The frontend `JsonSerializer` we have support the concept of deserialize and serialize things correctly.
Building on the information from the Field decorator, it should be able to deserialize into the correct types
for each of the keys in a payload.

## Specs

We will need specs for both the backend and the frontend to verify everything, including specs for the new types introduced.

## Documentation

Add documentation pages in the serialization folders for both the backend and frontend with reference between them.
