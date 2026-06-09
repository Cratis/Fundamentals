// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Geospatial;

namespace Cratis.Json.for_LineStringJsonConverter;

public class when_converting_to_linestring : Specification
{
    LineStringJsonConverter converter;
    LineString expected;
    LineString result;

    void Establish()
    {
        converter = new();
        expected = new LineString(
        [
            new Point(10.5, 20.3),
            new Point(11.5, 21.3),
            new Point(12.5, 22.3)
        ]);
    }

    void Because()
    {
        const string json = "{\"type\":\"LineString\",\"coordinates\":[[10.5,20.3],[11.5,21.3],[12.5,22.3]]}";
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json).AsSpan());
        reader.Read();  // Move to start object
        result = converter.Read(ref reader, typeof(LineString), Globals.JsonSerializerOptions);
    }

    [Fact] void should_convert_to_correct_linestring()
    {
        result.Coordinates.Length.ShouldEqual(expected.Coordinates.Length);
        for (var i = 0; i < result.Coordinates.Length; i++)
        {
            result.Coordinates[i].ShouldEqual(expected.Coordinates[i]);
        }
    }
}
