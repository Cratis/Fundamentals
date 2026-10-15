// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns_in_context;

public class with_an_offset_crossing_a_leap_day : Specification
{
    string _invoice;
    string _date;
    GenerationContext _context;

    void Establish() => _context = new(new DateTimeOffset(2024, 3, 1, 0, 30, 0, TimeSpan.FromHours(1)), GenerationContext.DefaultTenant);

    void Because()
    {
        _invoice = GenerateValue.Pattern("INV-{yyyy}-#{7}", _context, Checksum.Luhn);
        _date = GenerateValue.Pattern("{yyyy}{yy}{MM}{dd}-{MM}{dd}-[AB]", _context);
    }

    [Fact] void should_use_the_utc_year_and_append_one_digit() => (_invoice.Length == 17 && _invoice.StartsWith("INV-2024-", StringComparison.Ordinal) && _invoice[9..].All(char.IsAsciiDigit)).ShouldBeTrue();
    [Fact] void should_include_the_literal_and_date_digits_in_the_checksum() => GenerateValue.HasValidChecksum(_invoice, Checksum.Luhn).ShouldBeTrue();
    [Fact] void should_use_the_same_utc_leap_day_for_every_segment() => _date.StartsWith("2024240229-0229-", StringComparison.Ordinal).ShouldBeTrue();
}
