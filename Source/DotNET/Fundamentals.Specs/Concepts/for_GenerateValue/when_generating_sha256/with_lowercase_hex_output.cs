// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_generating_sha256;

public class with_lowercase_hex_output : Specification
{
    JsonElement[] _vectors;
    string[] _results;

    void Establish() => _vectors = GenerationVectors.Read("sha-256.json", "digests");

    void Because() => _results = [.. _vectors.Select(vector => GenerateValue.Sha256(GenerationVectors.Text(vector, "text")))];

    [Fact] void should_have_64_characters_for_every_vector() => _results.All(result => result.Length == 64).ShouldBeTrue();
    [Fact] void should_only_use_lowercase_hex_for_every_vector() => _results.All(result => result.All(character => character is (>= '0' and <= '9') or (>= 'a' and <= 'f'))).ShouldBeTrue();
}
