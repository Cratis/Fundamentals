---
description: "@cratis/fundamentals is the npm package of TypeScript utilities for typed concepts, serialization, geospatial types and dates, matching the .NET package."
---
# TypeScript (JavaScript/TypeScript)

`@cratis/fundamentals` is the [npm package](https://www.npmjs.com/package/@cratis/fundamentals) of common utilities, formalizations and abstractions for JavaScript and TypeScript applications, mirroring the .NET `Cratis.Fundamentals` package.

## Overview

The Fundamentals frontend package provides JavaScript/TypeScript equivalents and enhancements to common development patterns and utilities. It includes tools for type management, serialization, field handling, and reactive programming that complement the backend .NET package.

## Topics

| Topic | Description |
| ------- | ----------- |
| [ConceptAs](./concept_as.md) | Domain-Driven Design pattern for creating strongly-typed domain identifiers and value objects that wrap primitive types. |
| [GUID](./guid.md) | GUID creation, manipulation, and utility functions for unique identifiers. |
| [Constructor](./constructor.md) | Runtime constructor type alias for class-based APIs and metadata-driven tooling. |
| [PropertyAccessor](./property_accessor.md) | Property accessor function type for typed property selection and path utilities. |
| [TimeSpan](./time_span.md) | .NET-compatible time interval parsing, formatting, and JSON serialization. |
| [ValueMap](./value_map.md) | A value-based map for complex object keys with stable lookup semantics. |
| [Serialization](./serialization/index.md) | Type-safe JSON serialization and deserialization system with polymorphic support and runtime type preservation. |

## Installation

To install the package:

```bash
npm install @cratis/fundamentals
```

## Module imports

Use the package root for shared types, or import from `@cratis/fundamentals/geospatial`,
`@cratis/fundamentals/json`, and `@cratis/fundamentals/reflection`. These entry points
work in Node.js ESM, CommonJS, and TypeScript projects using NodeNext or Bundler
module resolution.

```typescript
import { Guid } from '@cratis/fundamentals';
import { Point } from '@cratis/fundamentals/geospatial';

const origin = new Point(0, 0);
const id = Guid.empty;
```
