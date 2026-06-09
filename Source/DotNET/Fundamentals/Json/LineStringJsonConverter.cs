// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Geospatial;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverter{T}"/> for <see cref="LineString"/> that follows GeoJSON format.
/// </summary>
public class LineStringJsonConverter : JsonConverter<LineString>
{
    /// <inheritdoc/>
    public override LineString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        string? type = null;
        List<Point>? coordinates = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (type != "LineString")
                {
                    throw new JsonException("Expected type to be 'LineString'");
                }
                if (coordinates == null || coordinates.Count < 2)
                {
                    throw new JsonException("LineString must have at least 2 points");
                }
                return new LineString([.. coordinates]);
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
                    coordinates = ReadLineStringCoordinates(ref reader);
                }
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, LineString value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "LineString");
        writer.WritePropertyName("coordinates");
        writer.WriteStartArray();
        foreach (var point in value.Coordinates)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(point.Longitude);
            writer.WriteNumberValue(point.Latitude);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    static List<Point> ReadLineStringCoordinates(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for coordinates");
        }

        var points = new List<Point>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return points;
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var coords = new List<double>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    if (reader.TokenType == JsonTokenType.Number)
                    {
                        coords.Add(reader.GetDouble());
                    }
                }

                if (coords.Count != 2)
                {
                    throw new JsonException("Each coordinate must have exactly 2 elements [longitude, latitude]");
                }

                points.Add(new Point(coords[0], coords[1]));
            }
        }

        throw new JsonException("Unexpected end of coordinates array");
    }
}
