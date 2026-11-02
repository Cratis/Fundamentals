---
title: Option
description: Return an optional configuration setting with Option<T>, then handle a present value or choose a default without unchecked access.
---

A missing retry-count setting can mean “use the default.” Throwing an exception for that case makes an ordinary lookup look like a failure. Returning `Option<string>` lets your method say that the text may be absent while leaving the default to its caller.

## Look up a setting

Use a .NET 10 SDK. In a new directory, create the console app and add the package:

```bash
dotnet new console --framework net10.0 --name OptionDemo
cd OptionDemo
dotnet add package Cratis.Fundamentals --version 7.32.0
```

Replace `Program.cs` with this complete program:

```csharp
using System;
using System.Collections.Generic;
using Cratis.Monads;

public static class OptionalSetting
{
    public static void Main()
    {
        var settings = new Dictionary<string, string>
        {
            ["timeoutSeconds"] = "30"
        };

        foreach (var key in new[] { "timeoutSeconds", "retryCount" })
        {
            var setting = FindSetting(settings, key);
            if (setting.TryGetValue(out var value))
            {
                Console.WriteLine($"{key}: {value}");
            }
            else
            {
                Console.WriteLine($"{key}: absent");
            }
        }

        var missing = FindSetting(settings, "retryCount");
        Console.WriteLine(missing.Match(value => value, _ => "Default: 3"));
        missing.Switch(value => Console.WriteLine(value), _ => Console.WriteLine("No retry setting"));
    }

    static Option<string> FindSetting(Dictionary<string, string> settings, string key)
    {
        if (settings.TryGetValue(key, out var value)) return value;
        return Option<string>.None();
    }
}
```

`FindSetting` returns the stored string through an implicit conversion to `Option<string>`. It creates absence explicitly with `None()`. `TryGetValue` gives the caller both the case check and the value; only use its `out` value inside the successful branch.

The last two calls show alternatives to an `if`. `Match` produces a value, while `Switch` runs an action for the selected case. Both take the value branch first and the none branch second. The `_` parameter ignores the none-case marker.

Run it:

```bash
dotnet run
```

Expected output:

```text
timeoutSeconds: 30
retryCount: absent
Default: 3
No retry setting
```

The lookup has not parsed either setting. It only answered whether text exists. The default of three retries belongs to this caller; another caller can choose a different default without changing the lookup.

## Avoid turning absence into an access error

`HasValue` reports whether the value case is active. `AsT0` accesses that case directly and throws if the option is none. Prefer `TryGetValue` or `Match` when the value may be absent.

`Option<T>` does not validate the contents of `T`. An empty string can be a present value, and the wrapper does not make the contents of an arbitrary reference type non-null. Use `Option<T>.None()` for absence instead of assigning `null` or relying on `default(Option<T>)`; the option itself is a reference type.

If a caller needs to know *why* a required setting is missing or invalid, absence is no longer enough information. Continue with [Result: reject an invalid setting](result.md). For a small private lookup whose nullable return value is already unambiguous, adding an option may not improve the contract.
