// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns;

public class with_a_readable_order_format : Specification
{
    string[] _values;

    void Because() => _values = [.. Enumerable.Range(0, 20).Select(_ => GenerateValue.Pattern("ORD-[A-HJ-NP-Z2-9]{4}-#{4,6}"))];

    [Fact] void should_preserve_literals_alphabets_and_repeat_bounds() => _values.All(IsOrderNumber).ShouldBeTrue();

    static bool IsOrderNumber(string value)
    {
        const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var digits = value.Length - 9;
        return value.StartsWith("ORD-", StringComparison.Ordinal) &&
            digits is >= 4 and <= 6 &&
            value[4..8].All(letters.Contains) &&
            value[8] == '-' &&
            value[9..].All(char.IsAsciiDigit);
    }
}
