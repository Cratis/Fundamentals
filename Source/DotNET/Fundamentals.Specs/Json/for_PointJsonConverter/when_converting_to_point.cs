// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Geospatial;

namespace Cratis.Json.for_PointJsonConverter;

public class when_converting_to_point : Specification
{
    PointJsonConverter converter;
    Point expected;
    Point result;

    void Establish()
    {
        converter = new();
        expected = new Point(10.5, 20.3);
    }

    void Because()
    {
        const string json = "{\"type\":\"Point\",\"coordinates\":[10.5,20.3]}";
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json).AsSpan());
        reader.Read();  // Move to start object
        result = converter.Read(ref reader, typeof(Point), Globals.JsonSerializerOptions);
    }

    [Fact] void should_convert_to_correct_point() => result.ShouldEqual(expected);
}
