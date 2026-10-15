// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_validating_checksums;

public class with_screenplay_vectors : Specification
{
    JsonElement[] _vectors;
    string[] _disagreements;

    void Establish() => _vectors = GenerationVectors.Read("generation-checksums.json", "validation");

    void Because() => _disagreements = [.. _vectors.Select(vector =>
    {
        var value = GenerationVectors.Text(vector, "value");
        var actual = GenerateValue.HasValidChecksum(value, GenerationVectors.Algorithm(vector, "algorithm"));

        return actual == vector.GetProperty("valid").GetBoolean() ? null : value;
    }).OfType<string>()];

    [Fact] void should_exercise_every_validation_vector() => _vectors.Length.ShouldEqual(24);
    [Fact] void should_validate_the_same_values() => _disagreements.ShouldBeEmpty();
}
