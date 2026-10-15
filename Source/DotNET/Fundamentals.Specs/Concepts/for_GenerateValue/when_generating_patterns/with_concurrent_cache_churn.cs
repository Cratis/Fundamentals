// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns;

public class with_concurrent_cache_churn : Specification
{
    ConcurrentBag<string> _invalid;

    void Establish() => _invalid = [];

    void Because() => Parallel.For(0, 400, index =>
    {
        var pattern = $"{index}-[AB]#";
        var value = GenerateValue.Pattern(pattern, Checksum.Mod11);
        if (!value.StartsWith($"{index}-", StringComparison.Ordinal) || !GenerateValue.HasValidChecksum(value, Checksum.Mod11))
        {
            _invalid.Add(value);
        }
    });

    [Fact] void should_publish_only_complete_counted_supports() => _invalid.ShouldBeEmpty();
}
