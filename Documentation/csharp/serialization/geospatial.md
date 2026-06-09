# Geospatial Serialization

Geospatial types (Point, LineString, Polygon) serialize to [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/) format — the standard representation for geographic data in JSON.

## Automatic Registration

When you use the Cratis Application Model, all geospatial converters are automatically registered in the global `JsonSerializerOptions`:

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

// Converters already registered — use directly
var point = new Point(10.5, 20.3);
var json = JsonSerializer.Serialize(point, Globals.JsonSerializerOptions);
```

## Point Serialization

Points serialize to GeoJSON Point format:

```csharp
var point = new Point(10.5, 20.3);
var json = JsonSerializer.Serialize(point, Globals.JsonSerializerOptions);
// {"type":"Point","coordinates":[10.5,20.3]}

var deserialized = JsonSerializer.Deserialize<Point>(json, Globals.JsonSerializerOptions);
```

**Validation:**
- `type` must be "Point"
- `coordinates` must be an array with exactly two numbers [longitude, latitude]

## LineString Serialization

LineStrings serialize to GeoJSON LineString format:

```csharp
var lineString = new LineString(
[
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);
var json = JsonSerializer.Serialize(lineString, Globals.JsonSerializerOptions);
// {"type":"LineString","coordinates":[[10.5,20.3],[11.2,21.1],[12.0,22.0]]}

var deserialized = JsonSerializer.Deserialize<LineString>(json, Globals.JsonSerializerOptions);
```

**Validation:**
- `type` must be "LineString"
- `coordinates` must be an array with at least two coordinate pairs
- Each pair must be an array of two numbers [longitude, latitude]

## Polygon Serialization

Polygons serialize to GeoJSON Polygon format with optional holes:

```csharp
var polygon = new Polygon(
    new LinearRing(
    [
        new Point(0, 0),
        new Point(10, 0),
        new Point(10, 10),
        new Point(0, 10),
        new Point(0, 0)
    ]),
    []
);
var json = JsonSerializer.Serialize(polygon, Globals.JsonSerializerOptions);
// {"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}

var deserialized = JsonSerializer.Deserialize<Polygon>(json, Globals.JsonSerializerOptions);
```

With holes:

```json
{
    "type": "Polygon",
    "coordinates": [
        [[0,0],[20,0],[20,20],[0,20],[0,0]],
        [[5,5],[15,5],[15,15],[5,15],[5,5]]
    ]
}
```

**Validation:**
- `type` must be "Polygon"
- `coordinates` must be an array with at least one ring (the shell)
- Each ring must have at least 4 coordinate pairs
- The first and last coordinate of each ring must be the same (closed ring)
- Each coordinate must be an array of two numbers [longitude, latitude]

## Manual Converter Configuration

If you need to configure converters outside the Application Model:

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new PointJsonConverter(),
        new LineStringJsonConverter(),
        new PolygonJsonConverter()
    }
};
```

## Validation Errors

If deserialization fails validation, a `JsonException` is thrown:

```csharp
try
{
    var json = "{\"type\":\"Point\",\"coordinates\":[10.5]}"; // Missing latitude
    var point = JsonSerializer.Deserialize<Point>(json, Globals.JsonSerializerOptions);
}
catch (JsonException ex)
{
    Console.WriteLine(ex.Message); // Validation failed
}
```

## Migration from Coordinate

The deprecated `Coordinate` type used a different JSON format. If you have existing data:

**Old format:**
```json
{"longitude": 10.5, "latitude": 20.3}
```

**New format (GeoJSON):**
```json
{"type": "Point", "coordinates": [10.5, 20.3]}
```

Replace all `Coordinate` references with `Point` and update your stored JSON accordingly. TypeScript maintains a deprecated `Coordinate` class for backwards compatibility, but it will be removed in a future version.
