# Work with Polygons

## Creating a Simple Polygon

Create a Polygon with just an outer boundary (no holes):

```csharp
using Cratis.Geospatial;

var park = new Polygon(
    new LinearRing(
    [
        new Point(0, 0),
        new Point(10, 0),
        new Point(10, 10),
        new Point(0, 10),
        new Point(0, 0) // Must close the ring
    ]),
    []
);
```

The first parameter is the outer boundary (shell) as a `LinearRing`. The second is an array of holes (inner boundaries) — pass an empty array if there are no holes.

**Important:** The first and last points of a LinearRing must be identical to close the ring.

## Creating a Polygon with Holes

Create a Polygon with excluded areas (holes):

```csharp
using Cratis.Geospatial;

var buildingFootprint = new Polygon(
    new LinearRing( // Outer boundary
    [
        new Point(0, 0),
        new Point(20, 0),
        new Point(20, 20),
        new Point(0, 20),
        new Point(0, 0)
    ]),
    [
        new LinearRing( // Courtyard (hole)
        [
            new Point(5, 5),
            new Point(15, 5),
            new Point(15, 15),
            new Point(5, 15),
            new Point(5, 5)
        ])
    ]
);
```

## Using Polygons in Models

Polygons work as properties in domain models:

```csharp
using Cratis.Geospatial;

public record Zone(
    string Name,
    string Type,
    Polygon Boundary
);

var parkingLot = new Zone(
    "Main Parking",
    "Parking",
    new Polygon(
        new LinearRing(
        [
            new Point(10.7522, 59.9139),
            new Point(10.7622, 59.9139),
            new Point(10.7622, 59.9239),
            new Point(10.7522, 59.9239),
            new Point(10.7522, 59.9139)
        ]),
        []
    )
);
```

## Serializing and Deserializing

The `PolygonJsonConverter` is automatically registered when you use the Cratis Application Model:

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

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
// Output: {"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}

var deserialized = JsonSerializer.Deserialize<Polygon>(json, Globals.JsonSerializerOptions);
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
        new PolygonJsonConverter()
    }
};
```

## Validation

The converter validates:
- The `type` property is "Polygon"
- The `coordinates` property is an array with at least one ring (the shell)
- Each ring has at least 4 coordinate pairs (minimum for a closed polygon)
- The first and last coordinate of each ring are the same (closed ring)
- Each coordinate pair is an array of two numbers [longitude, latitude]

If validation fails, a `JsonException` is thrown.

## Accessing Shell and Holes

Once created, you can access the shell and holes:

```csharp
var polygon = new Polygon(
    new LinearRing([...]),
    [new LinearRing([...])]
);

var shell = polygon.Shell;     // Outer boundary
var holes = polygon.Holes;     // Array of holes
Console.WriteLine(holes.Length); // Number of holes
```
