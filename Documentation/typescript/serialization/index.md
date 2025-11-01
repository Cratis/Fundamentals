# Serialization

The Fundamentals frontend package provides a comprehensive serialization system that enables type-safe JSON serialization and deserialization in TypeScript. This system maintains object identity, method access, and polymorphic type handling throughout the serialization process.

## Overview

The serialization system consists of two main components that work together to provide full-featured object serialization:

- **Field Decorator System**: Provides runtime type metadata for proper deserialization
- **DerivedTypes System**: Enables polymorphic serialization with type identifiers

## Key Features

- **True Runtime Types**: Deserialized objects are actual class instances, not plain JavaScript objects
- **Method Preservation**: Full access to class methods and business logic on deserialized instances
- **Polymorphic Support**: Automatic resolution of derived types based on type identifiers
- **Type-Safe Collections**: Proper typing for arrays and complex nested objects
- **Deep Object Graphs**: Maintaining correct types throughout complex object hierarchies
- **Backend Integration**: Seamless compatibility with .NET DerivedTypes system

## Topics

| Topic | Description |
| ------- | ----------- |
| [JsonSerializer](./json_serializer.md) | The core utility for type-safe JSON serialization and deserialization with runtime type preservation and method access. |
| [Field Decorator](./field_decorator.md) | The `@field` decorator for runtime type information and proper object deserialization with full type safety. |
| [DerivedTypes](./derived_types.md) | Polymorphic serialization system that works with backend .NET implementation for type-safe object resolution. |

## Quick Start

### Basic Field Declaration

```typescript
import { field } from '@cratis/fundamentals';

export class User {
    @field(String)
    name!: string;
    
    @field(Number)
    age!: number;
    
    @field(Date)
    createdAt!: Date;
    
    // Methods are preserved after deserialization
    getDisplayName(): string {
        return `${this.name} (${this.age})`;
    }
}
```

### Polymorphic Types

```typescript
import { derivedType, field } from '@cratis/fundamentals';

@derivedType('550e8400-e29b-41d4-a716-446655440001')
export class CreditCard implements IPaymentMethod {
    @field(Number)
    amount!: number;
    
    @field(String)
    cardNumber!: string;
}

@derivedType('550e8400-e29b-41d4-a716-446655440002')
export class PayPal implements IPaymentMethod {
    @field(Number)
    amount!: number;
    
    @field(String)
    email!: string;
}
```

For detailed usage examples and advanced scenarios, see the individual topic documentation linked above.
