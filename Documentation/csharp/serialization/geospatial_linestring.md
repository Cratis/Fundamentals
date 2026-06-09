# Geospatial LineString

The `LineString` record represents a connected sequence of two or more points forming a path or line following the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson).

## What is a LineString?

A LineString is a linear feature that connects multiple points in order, representing paths, routes, or boundaries. Unlike a simple array of points, a LineString explicitly represents the connection between consecutive points, forming a continuous line.

The line must have at least two points and represents the shortest path (geodesic) between each consecutive pair of points on Earth's surface.

## Use Cases

LineString is useful for:
- **Routes and paths**: Hiking trails, bike routes, driving directions, walking paths
- **Boundaries**: Rivers, roads, property lines, administrative borders
- **Connections**: Network cables, pipeline routes, power lines, fiber optic networks
- **Trajectories**: Vehicle tracks, flight paths, ship routes, movement history
- **Transportation**: Bus routes, train lines, delivery routes
- **Infrastructure**: Roads, railways, utility lines

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
