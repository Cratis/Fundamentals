---
title: Fundamentals
description: The shared building blocks beneath the Cratis stack — concepts, serialization, dependency injection, and type discovery, for both .NET and TypeScript.
---

Fundamentals is the layer of shared building blocks that the rest of Cratis is built on. It's a convenience layer over .NET and TypeScript that solves the same small problems every project hits — strongly-typed domain values, JSON serialization, convention-based DI, runtime type discovery — so you don't reimplement them.

You'll meet it indirectly all the time: the [`ConceptAs<T>`](./csharp/index.md) you wrap an `AuthorId` in, the serialization that carries events across the wire, and the type discovery that lets [Chronicle](/chronicle/) and [Arc](/arc/) find your artifacts by convention all live here. You can also use it on its own.

Both platforms are supported, but there's no requirement for feature parity — each leans on the idioms of its environment.

## Packages

[![Nuget](https://img.shields.io/nuget/v/Cratis.Fundamentals?logo=nuget)](http://nuget.org/packages/cratis.fundamentals)
[![NPM](https://img.shields.io/npm/v/@cratis/fundamentals?label=@cratis/fundamentals&logo=npm)](https://www.npmjs.com/package/@cratis/fundamentals)

## What's Included

### Common Features (Both Platforms)

- **Concepts**: Domain-driven design patterns for type-safe, domain-specific abstractions
- **Serialization**: Robust JSON serialization with support for custom type handling
- **Dependency Injection**: Convention-based service registration and dependency management
- **Type Discovery**: Runtime type resolution and reflection utilities
- **Utilities**: Collections of helper functions and extension methods for common tasks

### .NET Specific

- **Metrics & Monitoring**: Comprehensive metrics system with compile-time source generation
- **Reflection**: Advanced type inspection and manipulation utilities
- **Monads**: Functional programming patterns for error handling and composition
- **Reactive Extensions**: Tools for working with reactive programming patterns
- **Task & Async Utilities**: Helpers for asynchronous programming patterns

### JavaScript/TypeScript Specific

- **GUID**: Efficient GUID/UUID utilities for TypeScript
- **Serialization**: Advanced JSON serialization with derived type support
- **Property Path Resolution**: Proxy-based property accessor for dynamic object access
- **Decorators**: Field and derived type decorators for object composition

## Documentation Sections

| Section | Description |
| ------- | ----------- |
| [C#](./csharp/index.md) | Documentation for the .NET package, including concepts, types, serialization, and more. |
| [TypeScript](./typescript/index.md) | Documentation for the JavaScript/TypeScript package, including utilities and abstractions. |

## Getting Started

Choose the appropriate section based on your development needs:

- **For .NET developers**: Start with the [C# documentation](./csharp/index.md)
- **For JavaScript/TypeScript developers**: Start with the [TypeScript documentation](./typescript/index.md)
