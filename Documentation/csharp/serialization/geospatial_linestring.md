# Geospatial LineString

The `LineString` record represents a line composed of two or more connected points. It provides GeoJSON-compliant JSON serialization support for both C# and TypeScript, making it easy to work with paths and routes.

## Basic Usage

Create a line string with multiple points:

```csharp
using Cratis.Geospatial;

var route = new LineString(
[
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);

Console.WriteLine($"Points in route: {route.Coordinates.Length}"); // 3
```

## JSON Serialization

The `LineStringJsonConverter` is automatically registered in the global `JsonSerializerOptions` when using the Cratis Application Model. It follows the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson) for LineString geometries.

### Serialization

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var lineString = new LineString(
[
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);
var json = JsonSerializer.Serialize(lineString, Globals.JsonSerializerOptions);
```

Output (GeoJSON format):

```json
{
    "type": "LineString",
    "coordinates": [
        [10.5, 20.3],
        [11.2, 21.1],
        [12.0, 22.0]
    ]
}
```

### Deserialization

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var json = "{\"type\":\"LineString\",\"coordinates\":[[10.5,20.3],[11.2,21.1],[12.0,22.0]]}";
var lineString = JsonSerializer.Deserialize<LineString>(json, Globals.JsonSerializerOptions);
```

### Validation

The converter validates that:
- The `type` property is "LineString"
- The `coordinates` property is an array with at least two coordinate pairs
- Each coordinate pair is an array of two numbers [longitude, latitude]

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
        new LineStringJsonConverter()
    }
};
```

> **Note**: If you're using the Cratis Application Model, the converter is automatically configured for ASP.NET pipelines and other parts that need it, such as the Cratis Kernel transports.

## Integration with Models

LineString can be used as a property in your domain models:

```csharp
using Cratis.Geospatial;

public record Route(
    string Name,
    LineString Path
);

var hikingTrail = new Route(
    "Mountain Trail",
    new LineString(
    [
        new Point(10.7522, 59.9139), // Start
        new Point(10.7625, 59.9245), // Waypoint 1
        new Point(10.7728, 59.9351)  // End
    ])
);
```

## Use Cases

LineString is useful for:
- **Routes and paths**: Hiking trails, bike routes, driving directions
- **Boundaries**: Rivers, roads, property lines
- **Connections**: Network cables, pipeline routes
- **Trajectories**: Vehicle tracks, flight paths
