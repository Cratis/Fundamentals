// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_ulid;

public class with_the_default_shape : Specification
{
    string _result;

    void Because() => _result = GenerateValue.Ulid();

    [Fact] void should_have_twenty_six_characters() => _result.Length.ShouldEqual(26);
    [Fact] void should_use_uppercase_crockford_base32() => _result.All(CrockfordDecoding.UpperAlphabet.Contains).ShouldBeTrue();
    [Fact] void should_have_zero_leading_padding_bits() => (_result[0] is >= '0' and <= '7').ShouldBeTrue();
}
