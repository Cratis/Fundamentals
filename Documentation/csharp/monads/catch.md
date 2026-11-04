---
title: Catch
description: Wrap a throwing parser explicitly with Catch<TResult, TError>, handle its three cases, and rethrow only a captured exception.
---

Sometimes you cannot replace a throwing dependency with a `TryParse`-style API. You still need to distinguish “the timeout violates our rule” from “the parser threw.” `Catch<Configuration, ConfigurationError>` carries those outcomes separately from success.

For a parser you control, prefer the [Result lesson's `TryParse` implementation](result.md). The throwing parser here gives you a small boundary to adapt without a file, network service, or other setup.

## Adapt the throwing call

Use a .NET 10 SDK and create a console app:

```bash
dotnet new console --framework net10.0 --name CatchDemo
cd CatchDemo
dotnet add package Cratis.Fundamentals --version 7.32.0
```

Replace `Program.cs` with the complete program below. Its configuration record and error enum belong to the example.

```csharp
using System;
using System.Globalization;
using Cratis.Monads;

public static class ParserBoundary
{
    public static void Main()
    {
        foreach (var input in new string?[] { "30", null, "0", "invalid" })
        {
            var result = ReadTimeout(input);
            Console.WriteLine(result.Match(
                configuration => $"Timeout: {configuration.TimeoutSeconds}",
                error => $"Rejected: {error}",
                exception => $"Parser exception: {exception.GetType().Name}"));

            try
            {
                result.RethrowError();
                Console.WriteLine("No exception to rethrow");
            }
            catch (FormatException)
            {
                Console.WriteLine("Observed rethrown FormatException");
            }
        }

        Catch completed = Catch.Success();
        Catch<int> parsed = Catch<int>.Success(30);
        Console.WriteLine($"Smaller forms: {completed.IsSuccess}, {parsed.IsSuccess}");
    }

    static Catch<Configuration, ConfigurationError> ReadTimeout(string? value)
    {
        if (value is null)
            return Catch<Configuration, ConfigurationError>.Failed(ConfigurationError.MissingTimeout);
        try
        {
            var seconds = int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (seconds is < 1 or > 300)
                return Catch<Configuration, ConfigurationError>.Failed(ConfigurationError.InvalidTimeout);
            return Catch<Configuration, ConfigurationError>.Success(new Configuration(seconds));
        }
        catch (FormatException exception)
        {
            return Catch<Configuration, ConfigurationError>.Failed(exception);
        }
    }

    public record Configuration(int TimeoutSeconds);
    public enum ConfigurationError { MissingTimeout, InvalidTimeout }
}
```

The `try` / `catch` around `int.Parse` does the actual exception handling. Returning `Failed(exception)` stores the caught exception in the third case. Missing text and an out-of-range integer use the second case, which contains a typed configuration error.

The caller supplies **three** branches to `Match`, in this order: success value, typed error, exception. `Switch` takes three actions in the same order. Checking only `IsSuccess == false` cannot tell you which failure case is active.

Run it:

```bash
dotnet run
```

Expected output:

```text
Timeout: 30
No exception to rethrow
Rejected: MissingTimeout
No exception to rethrow
Rejected: InvalidTimeout
No exception to rethrow
Parser exception: FormatException
Observed rethrown FormatException
Smaller forms: True, True
```

The second `try` / `catch`, in `Main`, demonstrates rethrowing after case handling. `RethrowError()` preserves the captured exception's stack trace. It does nothing for success or a typed error. Despite its name, it does not throw your `ConfigurationError` value.

## Choose a smaller form when there is no domain error

`Catch` represents completion without a value or an exception. Create its cases with `Catch.Success()` and `Catch.Failed(exception)`.

`Catch<TResult>` represents a result value or an exception. Use `Catch<TResult>.Success(value)` and `Catch<TResult>.Failed(exception)`. Both of these forms have two branches in `Match` and `Switch`: success first, exception second. A no-value success branch receives a case marker that you can ignore.

`Catch<TResult, TError>` adds the typed-error case. Its `Failed` factory is overloaded for `TError` and `Exception`, so choose distinct payload types. The generic forms also accept their payloads through implicit conversions; the factories make the case explicit at a boundary.

All three forms expose `IsSuccess`, `TryGetException`, and `RethrowError`. The value-bearing forms add `TryGetResult`; the three-case form adds `TryGetError`. Check the boolean returned by a `TryGet…` call before using its output. Direct `AsT0` / `AsT1` / `AsT2` access throws for an inactive case; in the three-case form, `AsT1` is the typed error and **`AsT2` is the exception**.

## Decide which exceptions cross the boundary

This adapter captures only `FormatException`. For example, text representing a number larger than `Int32.MaxValue` makes `int.Parse` throw `OverflowException`, which escapes this method. `Catch` never intercepts it automatically. If oversized input should be a normal rejection in your application, use `TryParse` as in the Result lesson or deliberately extend the adapter's policy.

The sample has no asynchronous operation or cancellation token. At a real asynchronous boundary, do not turn cancellation into an ordinary configuration failure by adding a blanket `catch (Exception)`. Keep cancellation observable to the caller. Likewise, do not catch programmer errors merely to force every outcome into a union.

You now have three different contracts for the setting: [Option](option.md) for absence, [Result](result.md) for a known rejection, and `Catch` for a deliberately captured exception. Choose at the boundary where your caller can use the distinction.
