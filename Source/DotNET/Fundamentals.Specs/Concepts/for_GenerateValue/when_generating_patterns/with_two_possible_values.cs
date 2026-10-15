// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns;

public class with_two_possible_values : Specification
{
    string[] _values;

    void Because() => _values = [.. Enumerable.Range(0, 200).Select(_ => GenerateValue.Pattern("[AB]"))];

    [Fact] void should_reach_both_values() => _values.Distinct().Order().ShouldEqual(["A", "B"]);
}
