// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Geospatial;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverter{T}"/> for <see cref="Point"/> that follows GeoJSON format.
/// </summary>
public class PointJsonConverter : JsonConverter<Point>
{
    /// <inheritdoc/>
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        string? type = null;
        double[]? coordinates = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (type != "Point")
                {
                    throw new JsonException("Expected type to be 'Point'");
                }
                if (coordinates == null || coordinates.Length != 2)
                {
                    throw new JsonException("Expected coordinates array with 2 elements [longitude, latitude]");
                }
                return new Point(coordinates[0], coordinates[1]);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                if (propertyName?.Equals("type", StringComparison.OrdinalIgnoreCase) == true)
                {
                    type = reader.GetString();
                }
                else if (propertyName?.Equals("coordinates", StringComparison.OrdinalIgnoreCase) == true)
                {
                    coordinates = ReadCoordinatesArray(ref reader);
                }
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "Point");
        writer.WritePropertyName("coordinates");
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Longitude);
        writer.WriteNumberValue(value.Latitude);
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    static double[] ReadCoordinatesArray(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for coordinates");
        }

        List<double> coords = [];
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return [.. coords];
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                coords.Add(reader.GetDouble());
            }
        }

        throw new JsonException("Unexpected end of coordinates array");
    }
}
