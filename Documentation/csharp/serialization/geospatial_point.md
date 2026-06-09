# Geospatial Point

The `Point` record represents a geographic location with longitude and latitude values. It provides GeoJSON-compliant JSON serialization support for both C# and TypeScript, making it easy to share geospatial data between frontend and backend.

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

If you have existing code using `Coordinate`, rename it to `Point`. The JSON format has changed from the old format to GeoJSON:

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
