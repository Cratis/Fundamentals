---
title: TypeScript fundamentals
description: Use Cratis Fundamentals in TypeScript to create identifiers, define typed domain values, and convert JSON in browser or Node.js applications.
---
<a id="typescript-javascripttypescript"></a>

`@cratis/fundamentals` provides typed domain values, GUIDs, dates, and JSON conversion for TypeScript applications. Use it in browser or Node.js code without a Cratis backend.

## Installation

Install the [npm package](https://www.npmjs.com/package/@cratis/fundamentals):

```bash
npm install @cratis/fundamentals
```

## Display an empty identifier

Add this to your TypeScript application's entry point:

```typescript
import { Guid } from '@cratis/fundamentals';

const emptyGuid = Guid.empty;
console.log(emptyGuid.toString());
```

Run the application with its usual development command. The console prints `00000000-0000-0000-0000-000000000000`, the empty GUID formatted as a string. This checks that the package import and identifier conversion work in your application.

## Overview

Choose a utility for the value or conversion your application needs:

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

## Next steps

- [Create and compare GUIDs](guid.md) — validate incoming identifiers and compare their values.
- [Define a typed concept](concept_as.md) — give a primitive value a domain-specific type.
- [Configure serialization](serialization/index.md) — preserve domain types when converting JSON.
