// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_nano_id;

public class with_a_custom_size_and_alphabet : Specification
{
    string _result;

    void Because() => _result = GenerateValue.NanoId(37, "abcde");

    [Fact] void should_respect_the_size() => _result.Length.ShouldEqual(37);
    [Fact] void should_use_only_the_custom_alphabet() => _result.All("abcde".Contains).ShouldBeTrue();
}
