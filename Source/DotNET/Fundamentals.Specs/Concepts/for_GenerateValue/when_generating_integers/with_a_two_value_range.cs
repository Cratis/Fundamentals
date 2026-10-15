// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_integers;

public class with_a_two_value_range : Specification
{
    long[] _values;

    void Because() => _values = [.. Enumerable.Range(0, 200).Select(_ => GenerateValue.Integer(0, 1))];

    [Fact] void should_reach_both_inclusive_endpoints() => _values.Distinct().Order().ShouldEqual([0L, 1L]);
}
