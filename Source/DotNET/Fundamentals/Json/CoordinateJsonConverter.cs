// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Geospatial;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverter{T}"/> for <see cref="Coordinate"/>.
/// </summary>
public class CoordinateJsonConverter : JsonConverter<Coordinate>
{
    /// <inheritdoc/>
    public override Coordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        double longitude = 0;
        double latitude = 0;
        var hasLongitude = false;
        var hasLatitude = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (!hasLongitude || !hasLatitude)
                {
                    throw new JsonException("Missing required properties for Coordinate");
                }
                return new Coordinate(longitude, latitude);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                if (propertyName?.Equals("longitude", StringComparison.OrdinalIgnoreCase) == true)
                {
                    longitude = reader.GetDouble();
                    hasLongitude = true;
                }
                else if (propertyName?.Equals("latitude", StringComparison.OrdinalIgnoreCase) == true)
                {
                    latitude = reader.GetDouble();
                    hasLatitude = true;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Coordinate value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(options.PropertyNamingPolicy?.ConvertName("Longitude") ?? "Longitude", value.Longitude);
        writer.WriteNumber(options.PropertyNamingPolicy?.ConvertName("Latitude") ?? "Latitude", value.Latitude);
        writer.WriteEndObject();
    }
}
