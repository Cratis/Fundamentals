---
description: Handle optional values, operation results and exceptions explicitly in .NET with the Option, Result and Catch monads.
---
# Monads

Cratis Fundamentals provides monads in the `Cratis.Fundamentals` package for returning optional values, expected failures and caught exceptions. Choose the return type according to what the caller needs to know: is a value absent, did an operation fail for a known reason, or did an exception-throwing API fail?

## Available Monads

- **[Option](option.md)** — Use `Option<T>` when a value may be absent. The user lookup example returns either a user or an explicit absence.
- **[Result](result.md)** — Use `Result<TResult, TError>` when success produces a value and expected failures have a typed explanation. The order examples also show variants for pass/fail alone or a typed error without a success value.
- **[Catch](catch.md)** — Use `Catch<TResult>` at an exception-throwing boundary. The file examples explicitly catch exceptions in the producer and return them for the caller to handle as values.

## Benefits

The method signature tells the caller which outcomes to consider before extracting a value. A missing user need not become a null dereference, and an expected order-validation failure need not become an exception. The examples show direct outcome checks as well as `Match` for returning another value and `Switch` for performing actions.

The return type does not catch exceptions on its own. In particular, `Catch` holds the outcome supplied by the producer; the producer still needs an explicit exception boundary.

## When to Use

Choose one of these contracts when designing a method's return type or adapting an existing API. They are alternatives, not a required three-step stack: a domain validation method can return `Result` while a separate file-access boundary returns `Catch`.

Follow the page for the outcome you need. Each page uses producer and consumer excerpts to connect the return type to the caller's choices. The excerpts omit application-specific setup and helpers; they are not standalone applications.
