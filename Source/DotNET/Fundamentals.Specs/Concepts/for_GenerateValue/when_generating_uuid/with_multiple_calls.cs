// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid;

public class with_multiple_calls : Specification
{
    Guid[] _results;

    void Because() => _results = [.. Enumerable.Range(0, 1000).Select(_ => GenerateValue.Uuid())];

    [Fact] void should_generate_distinct_values() => _results.Distinct().Count().ShouldEqual(1000);
}
