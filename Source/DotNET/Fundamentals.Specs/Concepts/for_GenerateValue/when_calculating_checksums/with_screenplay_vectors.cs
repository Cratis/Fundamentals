// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_calculating_checksums;

public class with_screenplay_vectors : Specification
{
    JsonElement[] _vectors;
    string[] _disagreements;

    void Establish() => _vectors = GenerationVectors.Read("generation-checksums.json", "checksums");

    void Because() => _disagreements = [.. _vectors.Select(vector =>
    {
        var payload = GenerationVectors.Text(vector, "payload");
        var expected = vector.GetProperty("suffix").GetString();
        var actual = GenerationChecksum.Calculate(payload, GenerationVectors.Algorithm(vector, "algorithm"));

        return actual == expected ? null : $"{payload}: expected {expected ?? "excluded"}, got {actual ?? "excluded"}";
    }).OfType<string>()];

    [Fact] void should_exercise_every_checksum_vector() => _vectors.Length.ShouldEqual(18);
    [Fact] void should_calculate_the_same_suffix_or_exclusion() => _disagreements.ShouldBeEmpty();
}
