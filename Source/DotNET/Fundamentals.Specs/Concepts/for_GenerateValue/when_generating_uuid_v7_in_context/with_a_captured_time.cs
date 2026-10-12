// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v7_in_context;

public class with_a_captured_time : Specification
{
    GenerationContext _context;
    byte[] _bytes;

    void Establish() => _context = new(DateTimeOffset.Parse("2024-06-15T12:34:56.789+02:00"), "tenant-a");
    void Because() => _bytes = GenerateValue.UuidV7(_context).ToByteArray(bigEndian: true);

    [Fact] void should_encode_the_captured_time() => CrockfordDecoding.Timestamp(_bytes).ShouldEqual(_context.Now.ToUnixTimeMilliseconds());
    [Fact] void should_have_version_seven() => (_bytes[6] >> 4).ShouldEqual(7);
    [Fact] void should_have_the_rfc_variant() => (_bytes[8] >> 6).ShouldEqual(2);
}
