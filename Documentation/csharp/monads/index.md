---
title: Monads
description: Choose Option for an absent setting, Result for a rejected value, or Catch for an exception at a .NET integration boundary.
---

A configuration lookup can find no setting. A setting can be present but invalid. A parser can throw. If all three become `null` or an exception, the caller has to guess what happened.

`Cratis.Monads` gives each outcome a place in the return type. You can use these types in a console app or library with only `Cratis.Fundamentals`; you do not need Arc, Chronicle, dependency injection, or a running service.

## Follow a setting from lookup to use

Start with a timeout stored as text. At each step, ask what the caller needs to distinguish:

1. **Look up the text.** An optional setting may be absent without anything going wrong. Return `Option<string>` so the caller can choose a default.
2. **Validate the text.** Once a timeout is required, missing text or an out-of-range number is a named rejection. Return `Result<Configuration, ConfigurationError>` so the caller can explain it.
3. **Adapt a throwing parser.** If an existing dependency throws, catch the exceptions you intend to represent and return `Catch<Configuration, ConfigurationError>`. The caller can distinguish a configuration error from a captured exception.

These are decisions you make in your methods. An `Option` does not validate a setting, a `Result` does not run an operation, and a `Catch` does not install an exception handler around your code.

| The caller needs to know | Return type | Cases, in `Match` / `Switch` order where supported |
| --- | --- | --- |
| Is there a value? | `Option<T>` | Value, none |
| Did it succeed, without a value or reason? | `Result` | Success, failure; inspect `IsSuccess` |
| Did it succeed, or which typed error occurred? | `Result<TError>` | Success without a value, error |
| Is there a value, or a typed error? | `Result<TResult, TError>` | Value, error |
| Did it complete, or which exception was captured? | `Catch` | Success without a value, exception |
| Is there a value, or a captured exception? | `Catch<TResult>` | Value, exception |
| Is there a value, a typed error, or a captured exception? | `Catch<TResult, TError>` | Value, error, exception |

The generic argument to `Result<TError>` is an **error** type. The argument to `Catch<TResult>` is a **success** type. With two arguments, the success type comes first in both families.

## Try an optional value

Use a .NET 10 SDK and create a console project. Pin the package so that you start with the API these examples use:

```bash
dotnet new console --framework net10.0 --name MonadsDemo
cd MonadsDemo
dotnet add package Cratis.Fundamentals --version 7.32.0
```

Replace `Program.cs` with this complete program:

```csharp
using System;
using Cratis.Monads;

public static class QuickStart
{
    public static void Main()
    {
        Option<string> timeout = "30";
        Console.WriteLine(timeout.Match(
            value => $"Configured: {value}",
            _ => "Use the default timeout"));
    }
}
```

The assignment uses the implicit conversion from a value to `Option<string>`. `Match` chooses one branch and returns its value. Both branches here return a string, so the caller gets a message regardless of whether the setting exists.

Run the program:

```bash
dotnet run
```

You should see:

```text
Configured: 30
```

Change the assignment to `Option<string> timeout = Option<string>.None();` and the second branch returns `Use the default timeout`. Absence is an explicit case; assigning `null` is not the way to create it.

## Choose the next lesson

- [Option: look up an optional setting](option.md) adds a real lookup and handles both presence and absence.
- [Result: reject an invalid setting](result.md) gives the caller a typed reason and introduces the no-value forms.
- [Catch: adapt a throwing parser](catch.md) keeps success, a domain rejection, and a captured exception separate.

You do not need functional-programming terminology to use these APIs. The chapter uses “monads” as the family name; the examples rely on explicit case handling through `TryGet…`, `Match`, and `Switch`. There is no automatic chaining or propagation between the three types.

## Keep the contract as small as the problem

Use a nullable value when absence is already clear and callers do not benefit from a separate case. Use a normal return value when the operation has only one meaningful outcome. For expected invalid input, a `TryParse`-style API may avoid exceptions entirely.

Choose a union when its cases help the caller make a decision. `Catch` is useful at a boundary with a throwing API, but wrapping every programming error as a value can conceal a bug. The [Catch lesson](catch.md#decide-which-exceptions-cross-the-boundary) shows where to draw that boundary.
