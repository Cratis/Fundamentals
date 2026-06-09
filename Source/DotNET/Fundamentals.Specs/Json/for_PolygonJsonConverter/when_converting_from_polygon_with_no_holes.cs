// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Geospatial;

namespace Cratis.Json.for_PolygonJsonConverter;

public class when_converting_from_polygon_with_no_holes : Specification
{
    PolygonJsonConverter converter;
    MemoryStream stream;
    Polygon input;
    Utf8JsonWriter writer;
    string result;

    void Establish()
    {
        converter = new();
        stream = new();
        var shell = new LinearRing(
        [
            new Point(0, 0),
            new Point(4, 0),
            new Point(4, 4),
            new Point(0, 4),
            new Point(0, 0)
        ]);
        input = new Polygon(shell, []);
        writer = new(stream);
    }

    void Because()
    {
        converter.Write(writer, input, Globals.JsonSerializerOptions);
        writer.Flush();
        result = Encoding.UTF8.GetString(stream.ToArray());
    }

    [Fact] void should_convert_to_geojson_format() => result.ShouldEqual("{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[4,0],[4,4],[0,4],[0,0]]]}");
}
