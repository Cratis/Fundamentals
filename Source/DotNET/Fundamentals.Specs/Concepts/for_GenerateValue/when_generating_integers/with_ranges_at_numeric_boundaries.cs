// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_integers;

public class with_ranges_at_numeric_boundaries : Specification
{
    bool[] _inRange;

    void Because()
    {
        (long Minimum, long Maximum)[] ranges =
        [
            (long.MinValue, long.MinValue), (long.MaxValue, long.MaxValue),
            (long.MinValue, long.MinValue + 2), (long.MaxValue - 2, long.MaxValue),
            (long.MinValue, 0), (-1, long.MaxValue), (-7, 3),
            (9007199254740992, 9007199254740993)
        ];
        _inRange = [.. ranges.Select(range => Enumerable.Range(0, 50).All(_ =>
        {
            var value = GenerateValue.Integer(range.Minimum, range.Maximum);

            return value >= range.Minimum && value <= range.Maximum;
        }))];
    }

    [Fact] void should_preserve_exact_integral_bounds_without_overflow() => _inRange.All(value => value).ShouldBeTrue();
}
