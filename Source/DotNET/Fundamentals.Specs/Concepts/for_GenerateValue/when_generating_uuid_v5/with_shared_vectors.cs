// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v5;

public class with_shared_vectors : Specification
{
    JsonElement[] _vectors;
    string[] _disagreements;

    void Establish() => _vectors = GenerationVectors.Read("uuid-v5.json", "uuids");

    void Because() => _disagreements = [.. _vectors.Select(vector =>
    {
        var @namespace = GenerationVectors.Text(vector, "namespace");
        var name = GenerationVectors.Text(vector, "name");
        var expected = GenerationVectors.Text(vector, "uuid");
        var actual = GenerateValue.UuidV5(Guid.Parse(@namespace), name).ToString();

        return string.Equals(actual, expected, StringComparison.Ordinal) ? null : $"{name}: expected {expected}, got {actual}";
    }).OfType<string>()];

    [Fact] void should_exercise_every_vector() => _vectors.Length.ShouldEqual(15);
    [Fact] void should_derive_the_same_uuid_for_every_vector() => _disagreements.ShouldBeEmpty();
}
