// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace Cratis.Json.for_DateOnlyJsonConverter;

public class when_converting_to_date_only_from_iso_string_from_datetimeoffset : Specification
{
    DateOnlyJsonConverter converter;
    string inputAsISOString;
    DateOnly result;

    void Establish()
    {
        converter = new();

        // Just after midnight at +01:00, so the written date (November 3rd) is a day after the UTC date (November 2nd).
        inputAsISOString = new DateTimeOffset(2026, 11, 3, 0, 30, 0, TimeSpan.FromHours(1)).ToString("O");
    }

    void Because()
    {
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes($"\"{inputAsISOString}\"").AsSpan());
        reader.Read();  // Skip quote
        result = converter.Read(ref reader, typeof(DateOnly), default);
    }

    [Fact] void should_convert_to_the_written_date() => result.ShouldEqual(new DateOnly(2026, 11, 3));
}
