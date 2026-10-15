// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_integers;

public class with_the_full_int64_range : Specification
{
    long[] _values;

    void Because() => _values = [.. Enumerable.Range(0, 200).Select(_ => GenerateValue.Integer(long.MinValue, long.MaxValue))];

    [Fact] void should_reach_negative_values() => _values.Any(value => value < 0).ShouldBeTrue();
    [Fact] void should_reach_positive_values() => _values.Any(value => value > 0).ShouldBeTrue();
}
