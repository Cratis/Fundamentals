// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using Cratis.Geospatial;

namespace Cratis.Json.for_CoordinateJsonConverter;

public class when_converting_to_coordinate : Specification
{
    CoordinateJsonConverter converter;
    Coordinate input;
    Coordinate result;

    void Establish()
    {
        converter = new();
        input = new Coordinate(10.5, 20.3);
    }

    void Because()
    {
        const string json = "{\"longitude\":10.5,\"latitude\":20.3}";
        Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json).AsSpan());
        reader.Read();  // Move to start object
        result = converter.Read(ref reader, typeof(Coordinate), Globals.JsonSerializerOptions);
    }

    [Fact] void should_convert_to_correct_coordinate() => result.ShouldEqual(input);
}
