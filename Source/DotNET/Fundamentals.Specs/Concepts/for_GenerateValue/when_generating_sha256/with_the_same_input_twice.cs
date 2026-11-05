// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_sha256;

public class with_the_same_input_twice : Specification
{
    string _value;
    string _first;
    string _second;
    string _different;

    void Establish() => _value = new string('a', 1024);

    void Because()
    {
        _first = GenerateValue.Sha256(_value);
        _second = GenerateValue.Sha256(_value);
        _different = GenerateValue.Sha256(_value + "b");
    }

    [Fact] void should_derive_the_same_digest() => _first.ShouldEqual(_second);
    [Fact] void should_derive_a_different_digest_for_a_one_character_change() => _different.ShouldNotEqual(_first);
}
