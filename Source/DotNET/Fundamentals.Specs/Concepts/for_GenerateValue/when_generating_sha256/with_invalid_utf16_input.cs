// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_generating_sha256;

public class with_invalid_utf16_input : Specification
{
    JsonElement[] _vectors;
    Exception[] _errors;

    void Establish() => _vectors = GenerationVectors.Read("sha-256.json", "invalid");

    void Because() => _errors = [.. _vectors.Select(vector =>
    {
        var value = new string([.. vector.GetProperty("utf16").EnumerateArray().Select(unit => (char)unit.GetInt32())]);

        return Catch.Exception(() => GenerateValue.Sha256(value));
    })];

    [Fact] void should_exercise_every_invalid_vector() => _vectors.Length.ShouldEqual(4);
    [Fact] void should_reject_every_invalid_input() => _errors.All(error => error is InvalidSha256Input).ShouldBeTrue();
}
