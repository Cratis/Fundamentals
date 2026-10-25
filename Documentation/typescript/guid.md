---
description: Create, parse, convert and compare GUIDs in TypeScript with the Guid class from @cratis/fundamentals.
---
# GUID

The `Guid` class provides robust GUID (Globally Unique Identifier) creation and manipulation utilities for JavaScript/TypeScript applications.

## Creating GUIDs

### New GUID

```typescript
import { Guid } from '@cratis/fundamentals';

// Create a new random GUID
const newId = Guid.create();
console.log(newId.toString()); // Example output: "550e8400-e29b-d441-a716-446655440000"
```

### From String

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

`Guid.isGuid()` checks the hyphenated `8-4-4-4-12` hexadecimal format. `Guid.parse()` converts a string but does not validate that format; validate untrusted input before parsing it.

## Equality

The `Guid` class implements proper equality comparison:

```typescript
import { Guid } from '@cratis/fundamentals';

const guid1 = Guid.parse('550e8400-e29b-41d4-a716-446655440000');
const guid2 = Guid.parse('550e8400-e29b-41d4-a716-446655440000');
const guid3 = Guid.parse('650e8400-e29b-41d4-a716-446655440000');

console.log(guid1.equals(guid2)); // true
console.log(guid1.equals(guid3)); // false
```

## Use Cases

GUIDs are commonly used for:

- **Entity Identifiers**: Unique identifiers for domain objects
- **Event IDs**: Tracking events in event-driven architectures

`Guid.create()` uses `Math.random()`, not a cryptographically secure random source. Do not use it for session tokens, API keys, or other secrets.

The GUID implementation ensures compatibility with .NET GUID formats, making it ideal for full-stack applications using both the frontend and backend Fundamentals packages.
