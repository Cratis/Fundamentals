---
description: Serialize .NET Point, LineString and Polygon types to and from GeoJSON, with converters registered automatically.
---
# Geospatial Serialization

Geospatial types (Point, LineString, Polygon) serialize to [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/) format — the standard representation for geographic data in JSON.

## Automatic Registration

The geospatial converters are registered in `Cratis.Json.Globals.JsonSerializerOptions`, so any code that uses these options gets them. The Cratis Application Model uses these options; a serializer configured separately must register the converters itself (see Manual Converter Configuration below):

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

// Converters already registered in the global options — use directly
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

**Required format and implemented checks:**

- `type` must be "Point" (checked)
- `coordinates` must be an array of two numbers [longitude, latitude]. The reader collects Number tokens and requires exactly two of them; other token kinds inside the array are skipped, not rejected

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

**Required format and implemented checks:**

- `type` must be "LineString" (checked)
- `coordinates` must be an array with at least two coordinate pairs (the point count is checked)
- Each pair must be an array of two numbers [longitude, latitude]. The reader processes each nested array, collects its Number tokens and requires exactly two; it skips other token kinds, including non-array items in the enclosing array, rather than rejecting every malformed shape

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

**Required format and implemented checks:**

- `type` must be "Polygon" (checked)
- `coordinates` must be an array with at least one ring (the shell) (checked)
- Each ring must have at least 4 coordinate pairs (checked)
- The first and last coordinate of each ring must match. Closure is checked as an absolute difference of at most `1e-9` for each coordinate, not exact equality
- Each coordinate must be an array of two numbers [longitude, latitude]. The reader processes nested arrays, collects Number tokens and requires exactly two; it skips other token kinds, including non-array items in the enclosing loops, rather than rejecting every malformed shape

## Manual Converter Configuration

If you use your own `JsonSerializerOptions` instead of `Globals.JsonSerializerOptions`:

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

The removed `Coordinate` type used a different JSON format. If you have existing data:

**Old format:**

```json
{"longitude": 10.5, "latitude": 20.3}
```

**New format (GeoJSON):**

```json
{"type": "Point", "coordinates": [10.5, 20.3]}
```

Replace all `Coordinate` references with `Point` and update your stored JSON accordingly.
