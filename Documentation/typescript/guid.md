---
description: Create, parse, convert and compare GUIDs in TypeScript with the Guid class from @cratis/fundamentals.
---
# GUID

Use `Guid` when an identifier needs to move between a string, bytes and a value you can compare. For an incoming string, check its shape with `Guid.isGuid()` before calling `Guid.parse()`. For two separately created instances of the same identifier, use `equals()` rather than reference equality.

## Creating GUIDs

### New GUID

```typescript
import { Guid } from '@cratis/fundamentals';

// Create a new random GUID
const newId = Guid.create();
console.log(newId.toString()); // Example output: "550e8400-e29b-d441-a716-446655440000"
```

### From String

`Guid.parse()` converts the supplied string; it does not reject malformed GUIDs. Validate external input with `Guid.isGuid()` first. The known identifier below needs no random generation:

```typescript
import { Guid } from '@cratis/fundamentals';

// Create from existing string
const existingId = Guid.parse('550e8400-e29b-41d4-a716-446655440000');
```

### From Uint8Array

```typescript
import { Guid } from '@cratis/fundamentals';

// The first three groups use little-endian byte order, as in .NET Guid.ToByteArray().
const bytes = new Uint8Array([0x00, 0x84, 0x0e, 0x55, 0x9b, 0xe2, 0xd4, 0x41, 0xa7, 0x16, 0x44, 0x66, 0x55, 0x44, 0x00, 0x00]);
const guidFromBytes = new Guid(bytes);
console.log(guidFromBytes.toString()); // "550e8400-e29b-41d4-a716-446655440000"
```

## Empty GUID

```typescript
import { Guid } from '@cratis/fundamentals';

// Get empty GUID (all zeros)
const emptyGuid = Guid.empty;
console.log(emptyGuid.toString()); // "00000000-0000-0000-0000-000000000000"
```

## String Conversion

```typescript
import { Guid } from '@cratis/fundamentals';

const guid = Guid.create();

// Convert to string (default format)
const str = guid.toString();

// Check if string is valid GUID format
const isValid = Guid.isGuid('550e8400-e29b-41d4-a716-446655440000'); // true
```

`Guid.isGuid()` checks the hyphenated `8-4-4-4-12` hexadecimal format, not whether an identifier exists in your application. `Guid.as()` is useful at a boundary accepting either a string or a `Guid`: it parses a string and passes an existing instance through unchanged. It does not add validation.

For a JSON request, `toJSON()` supplies the string automatically. For example, `JSON.stringify({ id: existingId })` produces `{"id":"550e8400-e29b-41d4-a716-446655440000"}` using the identifier from [From String](#from-string). Plain `JSON.parse()` reads that value back as a string, not a `Guid` instance.

## Equality

`equals()` compares the string representations, so independently parsed instances can match:

```typescript
import { Guid } from '@cratis/fundamentals';

const guid1 = Guid.parse('550e8400-e29b-41d4-a716-446655440000');
const guid2 = Guid.parse('550e8400-e29b-41d4-a716-446655440000');
const guid3 = Guid.parse('650e8400-e29b-41d4-a716-446655440000');

console.log(guid1.equals(guid2)); // true
console.log(guid1.equals(guid3)); // false
```

Check nullable inputs before calling `equals()`. Its argument is an `object`, not a nullable overload; for a non-`Guid` argument it calls `toString()`, so `null` or `undefined` is not a safe comparison input.

## Use Cases

GUIDs are commonly used for:

- **Entity Identifiers**: Unique identifiers for domain objects
- **Event IDs**: Tracking events in event-driven architectures

`Guid.create()` uses `Math.random()`, not a cryptographically secure random source. Do not use it for session tokens, API keys, or other secrets.

If an API response should restore an identifier as a `Guid` inside a model, declare its field with `@field(Guid)` and use [JsonSerializer](./serialization/json_serializer.md). That supplies a runtime constructor where plain JSON parsing only supplies a string.
