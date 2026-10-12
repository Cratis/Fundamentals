// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_bytes;

public class with_hex_encoding : Specification
{
    string[] _values;

    void Because() => _values = [.. Enumerable.Range(1, 512).Select(count => GenerateValue.Bytes(count, ValueEncoding.Hex))];

    [Fact] void should_encode_two_characters_per_byte() => _values.Select((value, index) => value.Length == (index + 1) * 2).All(matches => matches).ShouldBeTrue();
    [Fact] void should_use_lowercase_hex() => _values.All(value => value.All("0123456789abcdef".Contains)).ShouldBeTrue();
    [Fact] void should_decode_to_the_requested_byte_count() => _values.Select((value, index) => Convert.FromHexString(value).Length == index + 1).All(matches => matches).ShouldBeTrue();
}
