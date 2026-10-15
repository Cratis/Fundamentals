// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue;

internal static class GenerationVectors
{
    internal static JsonElement[] Read(string file, string section)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Concepts", "for_GenerateValue", "Vectors", file);
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        return [.. document.RootElement.GetProperty(section).EnumerateArray().Select(vector => vector.Clone())];
    }

    internal static string Text(JsonElement vector, string name) => vector.GetProperty(name).GetString()!;
    internal static Checksum Algorithm(JsonElement vector, string property = "checksum") =>
        !vector.TryGetProperty(property, out var value) ? Checksum.None : value.GetString() switch
        {
            "luhn" => Checksum.Luhn,
            "mod11" => Checksum.Mod11,
            "mod97" => Checksum.Mod97,
            _ => throw new UnknownChecksum((Checksum)(-1))
        };

    internal static DateOnly? Date(JsonElement vector) => vector.TryGetProperty("date", out var date)
        ? DateOnly.Parse(date.GetString()!, CultureInfo.InvariantCulture) : null;

    internal static DateTimeOffset? Instant(JsonElement vector) => vector.TryGetProperty("instant", out var instant)
        ? DateTimeOffset.Parse(instant.GetString()!, CultureInfo.InvariantCulture) : null;
}
