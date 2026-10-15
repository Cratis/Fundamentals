// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_parsing_patterns;

public class with_invalid_screenplay_vectors : Specification
{
    JsonElement[] _vectors;
    string[] _disagreements;

    void Establish() => _vectors = GenerationVectors.Read("generation-patterns.json", "invalid");

    void Because() => _disagreements = [.. _vectors.Select(vector =>
    {
        var pattern = GenerationVectors.Text(vector, "pattern");
        var reason = GenerationVectors.Text(vector, "reason");
        var error = Catch.Exception(() => GenerateValue.Pattern(pattern, GenerationVectors.Algorithm(vector)));

        return error is InvalidValuePattern && error.Message.Contains(pattern, StringComparison.Ordinal) && error.Message.Contains(reason, StringComparison.Ordinal)
            ? null : $"{pattern}: expected '{reason}', got {error}";
    }).OfType<string>()];

    [Fact] void should_exercise_every_invalid_vector() => _vectors.Length.ShouldEqual(25);
    [Fact] void should_reject_each_pattern_with_the_contract_reason() => _disagreements.ShouldBeEmpty();
}
