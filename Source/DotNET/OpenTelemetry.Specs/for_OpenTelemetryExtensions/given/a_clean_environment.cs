// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.given;

public class a_clean_environment : Specification
{
    Dictionary<string, string?> _original;

    void Establish()
    {
        _original = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
            .Where(entry => ((string)entry.Key).StartsWith("OTEL_", StringComparison.Ordinal))
            .ToDictionary(entry => (string)entry.Key, entry => entry.Value?.ToString());
        foreach (var key in _original.Keys)
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }

    void Destroy()
    {
        foreach (var key in Environment.GetEnvironmentVariables().Keys.Cast<string>().Where(key => key.StartsWith("OTEL_", StringComparison.Ordinal)))
        {
            Environment.SetEnvironmentVariable(key, null);
        }
        foreach (var (key, value) in _original)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
