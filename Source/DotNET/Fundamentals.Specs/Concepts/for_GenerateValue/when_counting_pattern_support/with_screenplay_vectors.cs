// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_counting_pattern_support;

public class with_screenplay_vectors : Specification
{
    JsonElement[] _vectors;
    string[] _disagreements;

    void Establish() => _vectors = GenerationVectors.Read("generation-patterns.json", "support");

    void Because() => _disagreements = [.. _vectors.Select(vector =>
    {
        var pattern = GenerationPattern.Parse(GenerationVectors.Text(vector, "pattern"), GenerationVectors.Algorithm(vector));
        var actual = new GenerationPatternSupport(pattern, GenerationVectors.Date(vector)).Count;
        var expected = BigInteger.Parse(GenerationVectors.Text(vector, "count"), CultureInfo.InvariantCulture);

        return actual == expected ? null : $"{pattern.Text}: expected {expected}, got {actual}";
    }).OfType<string>()];

    [Fact] void should_exercise_every_support_vector() => _vectors.Length.ShouldEqual(19);
    [Fact] void should_count_the_same_distinct_strings() => _disagreements.ShouldBeEmpty();
}
