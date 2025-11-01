# Fundamentals

Fundamentals is a comprehensive collection of packages that provides common utilities, formalizations, and abstractions to accelerate development in both backend (.NET) and frontend (JavaScript/TypeScript) environments.

## Overview

The Cratis Fundamentals library acts as a convenience layer on top of the existing base platforms (.NET and JavaScript/TypeScript), offering solutions to common development problems.
Rather than reimplementing basic functionality across multiple projects, Fundamentals provides battle-tested, reusable components that help developers be more productive.

While both platforms are supported, there is no requirement for feature parity.
Each platform leverages the unique building blocks and idioms of its environment to provide the most idiomatic solutions.

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
