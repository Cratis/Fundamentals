// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_matching_pattern_shapes;

public class with_screenplay_vectors : Specification
{
    JsonElement[] _vectors;
    string[] _disagreements;

    void Establish() => _vectors = GenerationVectors.Read("generation-patterns.json", "membership");

    void Because() => _disagreements = [.. _vectors.Select(vector =>
    {
        var pattern = GenerationPattern.Parse(GenerationVectors.Text(vector, "pattern"), GenerationVectors.Algorithm(vector));
        var value = GenerationVectors.Text(vector, "value");
        var actual = GenerationPatternSupport.Matches(pattern, value, GenerationVectors.Instant(vector));

        return actual == vector.GetProperty("valid").GetBoolean() ? null : $"{pattern.Text}: {value}";
    }).OfType<string>()];

    [Fact] void should_exercise_every_membership_vector() => _vectors.Length.ShouldEqual(10);
    [Fact] void should_match_the_same_shapes_without_implied_checksum_validation() => _disagreements.ShouldBeEmpty();
}
