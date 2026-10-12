// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_nano_id;

public class with_valid_boundaries : Specification
{
    string _smallest;
    string _largest;
    string _alphabet;
    string _result;

    void Establish() => _alphabet = new string([.. Enumerable.Range(0x20, 95).Select(value => (char)value)]);
    void Because()
    {
        _smallest = GenerateValue.NanoId(1, " ~");
        _largest = GenerateValue.NanoId(256, " ~");
        _result = GenerateValue.NanoId(256, _alphabet);
    }

    [Fact] void should_accept_the_smallest_size() => _smallest.Length.ShouldEqual(1);
    [Fact] void should_accept_the_largest_size() => _largest.Length.ShouldEqual(256);
    [Fact] void should_accept_all_printable_ascii_characters() => _result.All(_alphabet.Contains).ShouldBeTrue();
}
