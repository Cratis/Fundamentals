// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_ulid_in_context;

public class with_a_captured_time : Specification
{
    GenerationContext _context;
    string[] _values;

    void Establish() => _context = new(DateTimeOffset.Parse("2024-06-15T12:34:56.789+02:00"), "tenant-a");
    void Because() => _values = [.. Enumerable.Range(0, 1000).Select(_ => GenerateValue.Ulid(_context))];

    [Fact] void should_encode_the_captured_time() => _values.All(value => CrockfordDecoding.Timestamp(CrockfordDecoding.Decode(value)) == _context.Now.ToUnixTimeMilliseconds()).ShouldBeTrue();
    [Fact] void should_share_the_ten_character_time_prefix() => _values.Select(value => value[..10]).Distinct().Count().ShouldEqual(1);
    [Fact] void should_keep_randomness_in_one_context() => _values.Distinct().Count().ShouldEqual(_values.Length);
}
