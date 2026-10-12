// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_type_id;

public class with_a_prefix : Specification
{
    string _result;
    byte[] _bytes;
    long _before;
    long _after;

    void Because()
    {
        _before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _result = GenerateValue.TypeId("user");
        _after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _bytes = CrockfordDecoding.Decode(_result[5..]);
    }

    [Fact] void should_preserve_the_prefix_and_separator() => _result[..5].ShouldEqual("user_");
    [Fact] void should_have_a_twenty_six_character_suffix() => _result.Length.ShouldEqual(31);
    [Fact] void should_use_lowercase_crockford_base32() => _result[5..].All(CrockfordDecoding.LowerAlphabet.Contains).ShouldBeTrue();
    [Fact] void should_have_zero_leading_padding_bits() => (_result[5] is >= '0' and <= '7').ShouldBeTrue();
    [Fact] void should_encode_a_version_seven_uuid() => (_bytes[6] >> 4).ShouldEqual(7);
    [Fact] void should_encode_the_rfc_variant() => (_bytes[8] >> 6).ShouldEqual(2);
    [Fact] void should_encode_the_current_time() => (CrockfordDecoding.Timestamp(_bytes) >= _before && CrockfordDecoding.Timestamp(_bytes) <= _after).ShouldBeTrue();
}
