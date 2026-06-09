// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

namespace Cratis.Json.for_PolygonJsonConverter;

public class when_converting_to_polygon_with_unclosed_ring : Specification
{
    Exception result;

    void Because() => result = Catch.Exception(() =>
    {
        // Ring is not closed - first point (0,0) != last point (10,0)
        const string json = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[10,0],[10,10],[0,10]]]}";
        JsonSerializer.Deserialize<Polygon>(json, Globals.JsonSerializerOptions);
    });

    [Fact] void should_throw_json_exception() => result.ShouldBeOfExactType<JsonException>();
    [Fact] void should_indicate_ring_must_be_closed() => result.Message.ShouldContain("must be closed");
}
