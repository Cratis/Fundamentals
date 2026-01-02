# Monads

Monads provide a functional programming approach to handling common patterns like optional values, operation results, and exception handling. They enable explicit and type-safe handling of success and failure paths without relying on exceptions or null values.

## Available Monads

Cratis.Fundamentals provides the following monads:

- **[Option](option.md)** - Represents optional values, eliminating the need for null checks
- **[Result](result.md)** - Represents operation outcomes with typed success and error states
- **[Catch](catch.md)** - Represents operations that may throw exceptions with type-safe exception handling

## Benefits

- **Type Safety**: Explicit handling of success and failure cases at compile time
- **Null Safety**: Eliminates null reference exceptions through explicit optional values
- **Clear Intent**: Makes success and failure paths explicit in the code
- **Functional Composition**: Enables chaining operations while maintaining error handling
- **Better Error Handling**: Provides alternatives to exception-based control flow

## When to Use

Use monads when:

- A value may or may not be present (use `Option<T>`)
- An operation can succeed or fail with specific error types (use `Result<TResult, TError>`)
- You want to handle exceptions as values rather than control flow (use `Catch<TResult>`)
- You want to make optional or error states explicit in your API
