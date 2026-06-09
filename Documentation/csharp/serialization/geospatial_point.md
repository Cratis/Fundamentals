# Geospatial Point

The `Point` record represents a geographic location defined by longitude and latitude coordinates following the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson).

## What is a Point?

A Point is the most fundamental geospatial type, representing a single location on Earth's surface. It's defined by:
- **Longitude**: The east-west position, ranging from -180° to 180°
- **Latitude**: The north-south position, ranging from -90° to 90°

Points are used to represent discrete locations like addresses, landmarks, sensors, or any entity with a specific geographic position.

## Use Cases

Points are ideal for:
- **Location tracking**: Store positions of vehicles, people, or assets
- **Points of interest**: Restaurants, stores, landmarks, monuments
- **Sensor data**: Weather stations, IoT devices, monitoring equipment
- **User locations**: Customer addresses, delivery points, meeting locations
- **Event locations**: Concerts, conferences, incidents
- **Asset management**: Warehouse locations, equipment positions

## Basic Usage

Create a point with longitude and latitude:

```csharp
using Cratis.Geospatial;

var location = new Point(10.5, 20.3);
Console.WriteLine($"Longitude: {location.Longitude}"); // 10.5
Console.WriteLine($"Latitude: {location.Latitude}");   // 20.3
```

## JSON Serialization

The `PointJsonConverter` is automatically registered in the global `JsonSerializerOptions` when using the Cratis Application Model. It follows the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson) for Point geometries.

### Serialization

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var point = new Point(10.5, 20.3);
var json = JsonSerializer.Serialize(point, Globals.JsonSerializerOptions);
```

Output (GeoJSON format):

```json
{
    "type": "Point",
    "coordinates": [10.5, 20.3]
}
```

### Deserialization

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var json = "{\"type\":\"Point\",\"coordinates\":[10.5,20.3]}";
var point = JsonSerializer.Deserialize<Point>(json, Globals.JsonSerializerOptions);
```

### Validation

The converter validates that:
- The `type` property is "Point"
- The `coordinates` property is an array with exactly two numbers [longitude, latitude]

If validation fails, a `JsonException` will be thrown.

## Manual Converter Configuration

If you need to manually configure the converter:

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new PointJsonConverter()
    }
};
```

> **Note**: If you're using the Cratis Application Model, the converter is automatically configured for ASP.NET pipelines and other parts that need it, such as the Cratis Kernel transports.

## Integration with Models

Point can be used as a property in your domain models:

```csharp
using Cratis.Geospatial;

public record Location(
    string Name,
    Point Position
);

var office = new Location(
    "Headquarters",
    new Point(10.7522, 59.9139) // Oslo, Norway
);
```

## Migration from Coordinate

> **⚠️ Breaking Change**: The `Coordinate` type and `CoordinateJsonConverter` have been completely removed from the C# codebase. There is no backward compatibility layer in C#.

If you have existing code using `Coordinate`, you must:
1. Rename all `Coordinate` references to `Point` in your C# code
2. Update any stored JSON data from the old format to GeoJSON format
3. Update any API contracts that consume or produce `Coordinate` JSON

**Old format (Coordinate):**

```json
{
    "longitude": 10.5,
    "latitude": 20.3
}
```

**New format (Point):**

```json
{
    "type": "Point",
    "coordinates": [10.5, 20.3]
}
```

For TypeScript, a deprecated `Coordinate` class is provided for backward compatibility, but it will be removed in a future version. See the [TypeScript Coordinate documentation](../../typescript/coordinate.md) for details.
