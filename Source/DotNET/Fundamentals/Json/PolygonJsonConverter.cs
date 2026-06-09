// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Geospatial;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverter{T}"/> for <see cref="Polygon"/> that follows GeoJSON format.
/// </summary>
public class PolygonJsonConverter : JsonConverter<Polygon>
{
    /// <inheritdoc/>
    public override Polygon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        string? type = null;
        List<LinearRing>? rings = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (type != "Polygon")
                {
                    throw new JsonException("Expected type to be 'Polygon'");
                }
                if (rings == null || rings.Count == 0)
                {
                    throw new JsonException("Polygon must have at least one ring (shell)");
                }

                var shell = rings[0];
                var holes = rings.Count > 1 ? rings.Skip(1).ToArray() : [];
                return new Polygon(shell, holes);
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
                    rings = ReadPolygonCoordinates(ref reader);
                }
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Polygon value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "Polygon");
        writer.WritePropertyName("coordinates");
        writer.WriteStartArray();

        // Write shell
        WriteLinearRing(writer, value.Shell);

        // Write holes
        foreach (var hole in value.Holes)
        {
            WriteLinearRing(writer, hole);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    static void WriteLinearRing(Utf8JsonWriter writer, LinearRing ring)
    {
        writer.WriteStartArray();
        foreach (var point in ring.Coordinates)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(point.Longitude);
            writer.WriteNumberValue(point.Latitude);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }

    static List<LinearRing> ReadPolygonCoordinates(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for coordinates");
        }

        var rings = new List<LinearRing>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return rings;
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                rings.Add(ReadLinearRing(ref reader));
            }
        }

        throw new JsonException("Unexpected end of coordinates array");
    }

    static LinearRing ReadLinearRing(ref Utf8JsonReader reader)
    {
        var points = new List<Point>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                if (points.Count < 4)
                {
                    throw new JsonException("LinearRing must have at least 4 points");
                }
                return new LinearRing([.. points]);
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

        throw new JsonException("Unexpected end of linear ring array");
    }
}
