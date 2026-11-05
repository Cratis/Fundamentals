---
title: Geospatial serialization for .NET
description: Configure Cratis Fundamentals to serialize Point, LineString and Polygon values as GeoJSON in .NET, with coordinates in longitude, latitude order.
---
<a id="geospatial-serialization"></a>

Cratis Fundamentals provides `System.Text.Json` converters that write `Point`, `LineString` and `Polygon` values as [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/) geometry objects and read them back into `Cratis.Geospatial` types. Use them when a receiving system expects coordinates in longitude, latitude order.

Start with the options your serializer uses: the global options already contain the converters, while separate options need [manual configuration](#manual-converter-configuration). The examples below serialize geometries themselves, not GeoJSON `Feature` or `FeatureCollection` wrappers. If your stored data uses the removed `Coordinate` type, follow [Migration from Coordinate](#migration-from-coordinate) before reading it as a `Point`.

## Automatic registration

Pass `Cratis.Json.Globals.JsonSerializerOptions` to use the registered converters without adding them yourself. Cratis Arc also includes them in its default JSON configuration. For naming and null handling in a model that contains a geometry, see the [JSON option defaults](./serialization.md#jsonserializeroptions).

This example uses the global options to write a point:

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

// Converters already registered in the global options — use directly
var point = new Point(10.5, 20.3);
var json = JsonSerializer.Serialize(point, Globals.JsonSerializerOptions);
```

## Point serialization

A `Point` holds one longitude and latitude pair. Longitude is the first constructor argument and the first value in the JSON `coordinates` array. The converter adds `"type":"Point"` so the receiving system can identify the geometry:

```csharp
var point = new Point(10.5, 20.3);
var json = JsonSerializer.Serialize(point, Globals.JsonSerializerOptions);
// {"type":"Point","coordinates":[10.5,20.3]}

var deserialized = JsonSerializer.Deserialize<Point>(json, Globals.JsonSerializerOptions);
```

After deserialization, `deserialized` holds a `Point` with longitude `10.5` and latitude `20.3`. Reversing the constructor arguments changes the location without changing the JSON shape.

The reader requires `type` to be `"Point"` and collects exactly two JSON number tokens from `coordinates`. Other token kinds inside the array are skipped, not rejected. Quoted numbers are not converted, and longitude and latitude ranges are not checked.

## LineString serialization

A `LineString` holds connected points, such as positions along a route. Its GeoJSON `coordinates` value is an array of longitude, latitude pairs:

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

Reading this payload with the same options restores the three points. The reader requires `type` to be `"LineString"` and at least two coordinate pairs. Within each pair, it collects exactly two JSON number tokens. It skips other token kinds, including non-array items in the enclosing array, so it does not reject every malformed shape.

These checks happen on reading. Constructing a `LineString` record or writing it does not enforce the two-point minimum.

## Polygon serialization

A `Polygon` holds an outer boundary, called the shell, and optional inner boundaries, called holes. Build each boundary as a `LinearRing`, repeating the first point at the end to close it. This polygon has a shell and no holes:

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

The converter writes the shell first and then any holes. In this JSON, the first ring is the outer boundary and the second is a hole:

```json
{
    "type": "Polygon",
    "coordinates": [
        [[0,0],[20,0],[20,20],[0,20],[0,0]],
        [[5,5],[15,5],[15,15],[5,15],[5,5]]
    ]
}
```

When reading, the converter requires:

- `type` set to `"Polygon"` and at least one ring in `coordinates`.
- At least four coordinate pairs per ring, each supplying two JSON number tokens in longitude, latitude order.
- A closed ring: the first and last longitude must differ by no more than `1e-9`, as must the first and last latitude.

The reader skips other token kinds, including non-array items in the enclosing loops; these checks do not reject every malformed shape. Constructing the records or writing them does not validate ring length or closure.

## Manual converter configuration

For separately configured options, add the converters from `Cratis.Json`. Pass these options to both `Serialize` and `Deserialize` in place of `Globals.JsonSerializerOptions` in the examples above:

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

## Validation errors

A failed read check throws `JsonException`. This payload has a longitude but no latitude, so the point converter rejects it:

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

For this example, the exception message is `Expected coordinates array with 2 elements [longitude, latitude]`. Correct the payload's coordinate count before reading it again. For polygons, also check that each ring has at least four points and repeats its first point at the end.

Coordinate readers do not apply `JsonNumberHandling` settings: quoted coordinate values are skipped rather than parsed as numbers. The writers call `Utf8JsonWriter.WriteNumberValue` directly, so use finite `double` values, not `NaN` or infinity.

## Migration from Coordinate

The removed `Coordinate` type wrote named longitude and latitude properties. Existing data in that format must change along with the C# type:

Old format:

```json
{"longitude": 10.5, "latitude": 20.3}
```

New format (GeoJSON):

```json
{"type": "Point", "coordinates": [10.5, 20.3]}
```

Replace `Coordinate` references with `Point` and update stored JSON to the GeoJSON object shape. Longitude remains first in the coordinate pair. Update consumers expecting the old named properties too; replacing the C# type alone does not preserve the wire format.
