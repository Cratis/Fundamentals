# Work with LineStrings

## Creating a LineString

Create a LineString by providing two or more Points:

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

A LineString must have at least two Points. The points are stored in order, representing the path they form.

## Using LineStrings in Models

LineStrings work as properties in domain models:

```csharp
using Cratis.Geospatial;

public record Route(
    string Name,
    LineString Path
);

var trail = new Route(
    "Mountain Trail",
    new LineString(
    [
        new Point(10.7522, 59.9139), // Start
        new Point(10.7625, 59.9245), // Waypoint 1
        new Point(10.7728, 59.9351)  // End
    ])
);
```

## Serializing and Deserializing

The `LineStringJsonConverter` is automatically registered when you use the Cratis Application Model:

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
// Output: {"type":"LineString","coordinates":[[10.5,20.3],[11.2,21.1],[12.0,22.0]]}

var deserialized = JsonSerializer.Deserialize<LineString>(json, Globals.JsonSerializerOptions);
```

## Manual Converter Configuration

If you need to configure the converter manually:

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

## Validation

The converter validates:
- The `type` property is "LineString"
- The `coordinates` property is an array with at least two coordinate pairs
- Each coordinate pair is an array of two numbers [longitude, latitude]

If validation fails, a `JsonException` is thrown.

## Accessing Coordinates

Once created, you can access the Points in a LineString:

```csharp
var route = new LineString(
[
    new Point(10.5, 20.3),
    new Point(11.2, 21.1)
]);

foreach (var point in route.Coordinates)
{
    Console.WriteLine($"({point.Longitude}, {point.Latitude})");
}
```
