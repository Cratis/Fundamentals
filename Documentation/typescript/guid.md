# GUID

The `Guid` class provides robust GUID (Globally Unique Identifier) creation and manipulation utilities for JavaScript/TypeScript applications.

## Creating GUIDs

### New GUID

```typescript
import { Guid } from '@cratis/fundamentals';

// Create a new random GUID
const newId = Guid.create();
console.log(newId.toString()); // "550e8400-e29b-41d4-a716-446655440000"
```

### From String

```typescript
import { Guid } from '@cratis/fundamentals';

// Create from existing string
const existingId = new Guid('550e8400-e29b-41d4-a716-446655440000');
```

### From Uint8Array

```typescript
import { Guid } from '@cratis/fundamentals';

// Create from byte array
const bytes = new Uint8Array([0x55, 0x0e, 0x84, 0x00, 0xe2, 0x9b, 0x41, 0xd4, 0xa7, 0x16, 0x44, 0x66, 0x55, 0x44, 0x00, 0x00]);
const guidFromBytes = new Guid(bytes);
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

## Equality

The `Guid` class implements proper equality comparison:

```typescript
import { Guid } from '@cratis/fundamentals';

const guid1 = new Guid('550e8400-e29b-41d4-a716-446655440000');
const guid2 = new Guid('550e8400-e29b-41d4-a716-446655440000');
const guid3 = Guid.create();

console.log(guid1.equals(guid2)); // true
console.log(guid1.equals(guid3)); // false
```

## Use Cases

GUIDs are commonly used for:

- **Entity Identifiers**: Unique identifiers for domain objects
- **Event IDs**: Tracking events in event-driven architectures  
- **Session Tokens**: Temporary unique identifiers
- **API Keys**: Unique identifiers for external services

The GUID implementation ensures compatibility with .NET GUID formats, making it ideal for full-stack applications using both the frontend and backend Fundamentals packages.
