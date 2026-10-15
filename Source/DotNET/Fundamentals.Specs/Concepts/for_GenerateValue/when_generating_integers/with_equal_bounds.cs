// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_integers;

public class with_equal_bounds : Specification
{
    long _value;

    void Because() => _value = GenerateValue.Integer(5, 5);

    [Fact] void should_return_the_only_value() => _value.ShouldEqual(5L);
}
