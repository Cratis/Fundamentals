---
title: Result
description: Validate a required timeout with Result<TResult, TError>, distinguish missing and invalid input, and choose a no-value result when appropriate.
---

Your timeout setting is now required and must be between 1 and 300 seconds. “No value” cannot tell the caller whether the setting was missing or the supplied text was invalid. A typed error gives it enough information to explain the rejection without parsing an exception message.

## Return the value or a reason

With a .NET 10 SDK, create a console project:

```bash
dotnet new console --framework net10.0 --name ResultDemo
cd ResultDemo
dotnet add package Cratis.Fundamentals --version 7.32.0
```

Replace `Program.cs` with this complete program. The configuration record and error enum are local to this example; they are not Fundamentals APIs.

```csharp
using System;
using System.Globalization;
using Cratis.Monads;

public static class ValidatedSetting
{
    public static void Main()
    {
        Result presence = CheckPresence(null);
        Console.WriteLine($"Plain success: {presence.IsSuccess}");

        Result<ConfigurationError> required = CheckRequired(null);
        if (required.TryGetError(out var error)) Console.WriteLine($"Required: {error}");
        Console.WriteLine($"Required success: {CheckRequired("30").IsSuccess}");

        foreach (var input in new string?[] { "30", null, "invalid", "0" })
        {
            var result = Validate(input);
            if (result.TryGetResult(out var configuration))
            {
                Console.WriteLine($"Timeout: {configuration.TimeoutSeconds}");
            }
            else if (result.TryGetError(out var failure))
            {
                Console.WriteLine($"Rejected: {failure}");
            }
        }

        Console.WriteLine(Validate("30").Match(
            configuration => $"Ready: {configuration.TimeoutSeconds}",
            failure => $"Rejected: {failure}"));
    }

    static Result CheckPresence(string? value) =>
        value is null ? Result.Failed() : Result.Success();

    static Result<ConfigurationError> CheckRequired(string? value) =>
        value is null
            ? Result<ConfigurationError>.Failed(ConfigurationError.MissingTimeout)
            : Result<ConfigurationError>.Success();

    static Result<Configuration, ConfigurationError> Validate(string? value)
    {
        if (value is null)
            return Result<Configuration, ConfigurationError>.Failed(ConfigurationError.MissingTimeout);
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds)
            || seconds is < 1 or > 300)
            return Result<Configuration, ConfigurationError>.Failed(ConfigurationError.InvalidTimeout);
        return Result<Configuration, ConfigurationError>.Success(new Configuration(seconds));
    }

    public record Configuration(int TimeoutSeconds);
    public enum ConfigurationError { MissingTimeout, InvalidTimeout }
}
```

`Validate` uses `int.TryParse` because malformed text is expected input. Missing text and invalid text become different enum cases. Only a valid number constructs a configuration. The `Result` type carries this decision; it does not run validation on its own.

The caller uses guarded access to print the value or error. The final `Match` is an alternative that returns a string from either branch. Its argument order is success first, error second. Use `Switch` with the same case order when each branch should perform an action instead of return a value.

Run the program:

```bash
dotnet run
```

Expected output:

```text
Plain success: False
Required: MissingTimeout
Required success: True
Timeout: 30
Rejected: MissingTimeout
Rejected: InvalidTimeout
Rejected: InvalidTimeout
Ready: 30
```

`"0"` parses as an integer but still fails the range rule. That rejection is as much part of the contract as malformed text.

## Pick the right Result shape

The first two checks illustrate smaller contracts. `CheckPresence` returns `Result`: it has no success value and no error payload, just `Success()` or `Failed()`. A boolean may be sufficient if that is all your caller needs.

`CheckRequired` returns `Result<ConfigurationError>`: success still has no value, but failure carries an error. Its check only establishes presence; it does not validate the number. `Result<Configuration, ConfigurationError>` adds the usable value and is the appropriate shape for `Validate`.

For either generic form, `IsSuccess` identifies the success case and `TryGetError` checks for the error. The two-argument form also has `TryGetResult`. Direct access with `AsT0`, `AsT1`, or an explicit cast throws when the selected case is not active. Guard the access or use `Match`.

The generic forms also support implicit conversion from their payload types. Named `Success` and `Failed` factories keep the intended case visible, especially when payload types overlap. Do not use the same type for success and error if you expect an implicit conversion to distinguish them.

## When failure is an exception

A typed result works well when rejection is an expected outcome your caller can act on. It does not catch exceptions thrown inside `Validate`, and it does not prevent a caller from ignoring an error. Keep exception handling at the boundary where you can make a meaningful decision.

If an existing parser throws and you need to preserve that exception separately from a configuration rejection, continue with [Catch: adapt a throwing parser](catch.md). Otherwise, this `TryParse`-based implementation is enough; you do not need to add another wrapper.
