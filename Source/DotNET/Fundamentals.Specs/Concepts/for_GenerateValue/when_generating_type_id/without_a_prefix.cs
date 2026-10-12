// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_type_id;

public class without_a_prefix : Specification
{
    string _result;

    void Because() => _result = GenerateValue.TypeId(string.Empty);

    [Fact] void should_have_only_the_suffix() => _result.Length.ShouldEqual(26);
    [Fact] void should_not_add_a_separator() => _result.Contains('_').ShouldBeFalse();
}
