// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Geospatial;

namespace Cratis.Json.for_PolygonJsonConverter;

public class when_converting_to_polygon_with_no_holes : Specification
{
    PolygonJsonConverter converter;
    Polygon expected;
    Polygon result;

    void Establish()
    {
        converter = new();
        var shell = new LinearRing(
        [
            new Point(0, 0),
            new Point(4, 0),
            new Point(4, 4),
            new Point(0, 4),
            new Point(0, 0)
        ]);
        expected = new Polygon(shell, []);
    }

    void Because()
    {
        const string json = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[4,0],[4,4],[0,4],[0,0]]]}";
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json).AsSpan());
        reader.Read();  // Move to start object
        result = converter.Read(ref reader, typeof(Polygon), Globals.JsonSerializerOptions);
    }

    [Fact] void should_convert_to_correct_polygon()
    {
        result.Shell.Coordinates.Length.ShouldEqual(expected.Shell.Coordinates.Length);
        for (var i = 0; i < result.Shell.Coordinates.Length; i++)
        {
            result.Shell.Coordinates[i].ShouldEqual(expected.Shell.Coordinates[i]);
        }
        result.Holes.Length.ShouldEqual(expected.Holes.Length);
    }
}
