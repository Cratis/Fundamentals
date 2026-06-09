# Geospatial Polygon

The `Polygon` record represents a bounded area on Earth's surface with an outer boundary (shell) and optional inner boundaries (holes) following the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson).

## What is a Polygon?

A Polygon is a two-dimensional surface feature that encloses an area. It consists of:
- **Shell (outer boundary)**: A closed LinearRing defining the outer edge of the polygon
- **Holes (inner boundaries)**: Optional closed LinearRings defining areas excluded from the polygon (like courtyards, lakes within land, etc.)

Each ring must be closed, meaning the first and last points must be identical. Rings are represented as LinearRing objects, which are ordered sequences of at least 4 points (with the last point repeating the first).

## Use Cases

Polygon is useful for:
- **Areas and regions**: Parks, buildings, administrative boundaries, neighborhoods
- **Zones**: Parking lots, restricted areas, service coverage, delivery zones
- **Property**: Land parcels, real estate boundaries, ownership boundaries
- **Environmental**: Protected areas, flood zones, vegetation coverage, wildlife habitats
- **Urban planning**: City blocks, development zones, zoning districts
- **Geofencing**: Define virtual perimeters for location-based triggers and alerts
- **Agriculture**: Field boundaries, crop areas, irrigation zones
- **Facilities**: Building footprints, campus boundaries, complex structures with courtyards

## Basic Usage

Create a simple polygon with just a shell (no holes):

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

Create a polygon with holes:

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

## JSON Serialization

The `PolygonJsonConverter` is automatically registered in the global `JsonSerializerOptions` when using the Cratis Application Model. It follows the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson) for Polygon geometries.

### Serialization

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
```

Output (GeoJSON format):

```json
{
    "type": "Polygon",
    "coordinates": [
        [
            [0, 0],
            [10, 0],
            [10, 10],
            [0, 10],
            [0, 0]
        ]
    ]
}
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

### Deserialization

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var json = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}";
var polygon = JsonSerializer.Deserialize<Polygon>(json, Globals.JsonSerializerOptions);
```

### Validation

The converter validates that:
- The `type` property is "Polygon"
- The `coordinates` property is an array with at least one ring (the shell)
- Each ring (LinearRing) has at least 4 coordinate pairs (minimum for a closed polygon)
- The first and last coordinate of each ring are the same (closed ring)
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
        new PolygonJsonConverter()
    }
};
```

> **Note**: If you're using the Cratis Application Model, the converter is automatically configured for ASP.NET pipelines and other parts that need it, such as the Cratis Kernel transports.

## Integration with Models

Polygon can be used as a property in your domain models:

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

## LinearRing

A `LinearRing` is a closed line that forms the boundary of a polygon. It must have at least 4 points, and the first and last points must be identical to close the ring.

```csharp
var ring = new LinearRing(
[
    new Point(0, 0),
    new Point(10, 0),
    new Point(10, 10),
    new Point(0, 10),
    new Point(0, 0) // Closes the ring
]);
```
