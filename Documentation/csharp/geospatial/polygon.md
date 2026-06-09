# Polygon

## What is a Polygon?

A Polygon represents a bounded area on Earth's surface using one or more closed rings of points. The outer ring defines the boundary; optional inner rings define excluded areas (holes, like courtyards or lakes within land).

A Polygon ring must have at least 4 points, with the first and last points identical to close the ring.

Use Polygons for areas and regions: park boundaries, property parcels, delivery zones, geofences, administrative boundaries.

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

Create a Polygon with excluded areas:

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

## Accessing Shell and Holes

Access the outer boundary and any holes:

```csharp
var polygon = new Polygon(
    new LinearRing([...]),
    [new LinearRing([...])]
);

var shell = polygon.Shell;      // Outer boundary
var holes = polygon.Holes;      // Array of holes
Console.WriteLine(holes.Length); // Number of holes
```

## LinearRing

A `LinearRing` is a closed line that forms the boundary of a polygon. It must have at least 4 points, with the first and last identical to close the ring:

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

## Serialization

Polygons automatically serialize to GeoJSON format. See [Geospatial Serialization](../serialization/geospatial.md) for details on JSON handling.
