// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v5;

public class with_version_five_and_the_rfc_variant : Specification
{
    JsonElement[] _vectors;
    byte[][] _results;

    void Establish() => _vectors = GenerationVectors.Read("uuid-v5.json", "uuids");

    void Because() => _results = [.. _vectors.Select(vector => GenerateValue.UuidV5(
        Guid.Parse(GenerationVectors.Text(vector, "namespace")),
        GenerationVectors.Text(vector, "name")).ToByteArray(bigEndian: true))];

    [Fact] void should_set_version_five_for_every_vector() => _results.All(bytes => bytes[6] >> 4 == 5).ShouldBeTrue();
    [Fact] void should_set_the_rfc_variant_for_every_vector() => _results.All(bytes => (bytes[8] & 0xc0) == 0x80).ShouldBeTrue();
}
