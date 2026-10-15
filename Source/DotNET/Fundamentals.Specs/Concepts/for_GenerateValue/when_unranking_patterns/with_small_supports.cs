// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Cratis.Concepts.for_GenerateValue.when_unranking_patterns;

public class with_small_supports : Specification
{
    readonly DateTimeOffset _instant = new(2024, 2, 29, 0, 0, 0, TimeSpan.Zero);
    string[] _disagreements;

    void Because()
    {
        (string Pattern, Checksum Checksum)[] cases =
        [
            ("[AB]{1,3}[AB]{1,3}", Checksum.None),
            ("[AAABBB]", Checksum.None),
            ("#{2}", Checksum.Mod11),
            ("[A0-2]{1,2}[A0-2]{1,2}", Checksum.Mod11),
            ("{yyyy}{yy}{MM}{dd}[AB]{1,2}", Checksum.Luhn),
            ("[12]{1,2}{dd}[12]{1,2}", Checksum.Mod11),
            ("[AB]{1,2}", Checksum.Mod97)
        ];
        _disagreements = [.. cases.Select(CheckBijection).OfType<string>()];
    }

    string? CheckBijection((string Pattern, Checksum Checksum) example)
    {
        var pattern = GenerationPattern.Parse(example.Pattern, example.Checksum);
        var support = new GenerationPatternSupport(pattern, DateOnly.FromDateTime(_instant.UtcDateTime));
        var values = new HashSet<string>(StringComparer.Ordinal);
        for (var index = BigInteger.Zero; index < support.Count; index++)
        {
            var value = support.Unrank(index);
            if (!values.Add(value))
            {
                return $"Duplicate {value} for {example.Pattern}";
            }

            if (!GenerationPatternSupport.Matches(pattern, value, _instant))
            {
                return $"Not a member: {value}";
            }

            if (example.Checksum != Checksum.None && !GenerateValue.HasValidChecksum(value, example.Checksum))
            {
                return $"Invalid check digits: {value}";
            }
        }

        return support.Count > 0 && values.Count == support.Count ? null : $"Wrong cardinality for {example.Pattern}";
    }

    [Fact] void should_bijectively_enumerate_every_admissible_string() => _disagreements.ShouldBeEmpty();
}
