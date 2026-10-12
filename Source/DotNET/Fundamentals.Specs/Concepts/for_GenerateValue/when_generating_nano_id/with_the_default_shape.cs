// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_nano_id;

public class with_the_default_shape : Specification
{
    string _result;

    void Because() => _result = GenerateValue.NanoId();

    [Fact] void should_have_twenty_one_characters() => _result.Length.ShouldEqual(21);
    [Fact] void should_use_the_default_alphabet() => _result.All("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-".Contains).ShouldBeTrue();
}
