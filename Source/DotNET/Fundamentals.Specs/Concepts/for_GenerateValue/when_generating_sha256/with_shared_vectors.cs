// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_generating_sha256;

public class with_shared_vectors : Specification
{
    JsonElement[] _vectors;
    string[] _disagreements;

    void Establish() => _vectors = GenerationVectors.Read("sha-256.json", "digests");

    void Because() => _disagreements = [.. _vectors.Select(vector =>
    {
        var text = GenerationVectors.Text(vector, "text");
        var expected = GenerationVectors.Text(vector, "sha256");
        var actual = GenerateValue.Sha256(text);

        return string.Equals(actual, expected, StringComparison.Ordinal) ? null : $"{text}: expected {expected}, got {actual}";
    }).OfType<string>()];

    [Fact] void should_exercise_every_vector() => _vectors.Length.ShouldEqual(21);
    [Fact] void should_hash_every_vector_to_the_expected_digest() => _disagreements.ShouldBeEmpty();
}
