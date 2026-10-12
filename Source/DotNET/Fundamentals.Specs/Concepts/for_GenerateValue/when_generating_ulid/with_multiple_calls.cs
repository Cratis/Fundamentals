// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_ulid;

public class with_multiple_calls : Specification
{
    string[] _values;

    void Because() => _values = [.. Enumerable.Range(0, 1000).Select(_ => GenerateValue.Ulid())];

    [Fact] void should_produce_distinct_values() => _values.Distinct().Count().ShouldEqual(_values.Length);
}
