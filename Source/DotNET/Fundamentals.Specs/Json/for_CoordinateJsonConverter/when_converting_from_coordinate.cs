// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Geospatial;

namespace Cratis.Json.for_CoordinateJsonConverter;

public class when_converting_from_coordinate : Specification
{
    CoordinateJsonConverter converter;
    MemoryStream stream;
    Coordinate input;
    Utf8JsonWriter writer;
    string result;

    void Establish()
    {
        converter = new();
        stream = new();
        input = new Coordinate(10.5, 20.3);
        writer = new(stream);
    }

    void Because()
    {
        converter.Write(writer, input, Globals.JsonSerializerOptions);
        writer.Flush();
        result = Encoding.UTF8.GetString(stream.ToArray());
    }

    [Fact] void should_convert_to_correct_json() => result.ShouldEqual("{\"longitude\":10.5,\"latitude\":20.3}");
}
