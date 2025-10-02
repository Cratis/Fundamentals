# Frontend (JavaScript/TypeScript)

Frontend documentation for Fundamentals JavaScript/TypeScript package - an [npm package](https://www.npmjs.com/package/@cratis/fundamentals) that offers common utilities, formalizations and abstractions.

## Overview

The Fundamentals frontend package provides JavaScript/TypeScript equivalents and enhancements to common development patterns and utilities. It includes tools for type management, serialization, field handling, and reactive programming that complement the backend .NET package.

## Topics

| Topic | Description |
| ------- | ----------- |
| [GUID](./guid.md) | GUID creation, manipulation, and utility functions for unique identifiers. |

## Installation

To install the package:

```bash
npm install @cratis/fundamentals
```

## Key Features

### Core Utilities

- **GUID Generation**: Robust GUID creation and manipulation utilities
- **Type System**: Dynamic type discovery and management
- **Property Access**: Advanced property path resolution and proxy handling

### Serialization

- **JSON Serialization**: Enhanced JSON serialization with type preservation
- **Type Discriminators**: Support for polymorphic serialization matching backend patterns
- **Custom Converters**: Extensible converter system for custom types

### Field Management

- **Dynamic Fields**: Runtime field definition and manipulation
- **Field Decorators**: TypeScript decorators for field metadata
- **Validation**: Field-level validation and type checking

### Reactive Programming

- **Observable Patterns**: Reactive programming utilities
- **Event Handling**: Enhanced event management systems
- **State Management**: Utilities for managing application state

## Getting Started

Here's a simple example of using the GUID utilities:

```typescript
import { Guid } from '@cratis/fundamentals';

// Create a new GUID
const id = Guid.create();

// Create from string
const existingId = new Guid('550e8400-e29b-41d4-a716-446655440000');

// Convert to string
const stringId = id.toString();
```

### Working with Fields

```typescript
import { field, Fields } from '@cratis/fundamentals';

class Person {
    @field(String)
    firstName!: string;

    @field(String)
    lastName!: string;

    @field(Number)
    age!: number;
}

// Get field information
const fields = Fields.getFor(Person);
console.log(fields); // Array of field metadata
```

### JSON Serialization

```typescript
import { JsonSerializer } from '@cratis/fundamentals';

const person = {
    firstName: 'John',
    lastName: 'Doe',
    age: 30
};

// Serialize with type information
const json = JsonSerializer.serialize(person);

// Deserialize with type restoration
const restored = JsonSerializer.deserialize(json, Person);
```

## Architecture

The frontend package is designed to work seamlessly with the backend .NET package, providing:

- **Type Compatibility**: Consistent type handling between frontend and backend
- **Serialization Compatibility**: JSON payloads that work with both systems
- **Shared Concepts**: Similar abstractions and patterns for consistency

## Development

For more detailed examples and API documentation, explore the source code in the `Source/JavaScript/` directory. The package includes comprehensive TypeScript definitions and follows modern JavaScript development practices.

## Testing

The package includes extensive test coverage using modern testing frameworks. Tests can be found alongside the source code in `for_*` directories following the same BDD-style pattern as the backend package.
