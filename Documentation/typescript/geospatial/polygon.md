# Polygon

## What is a Polygon?

A Polygon represents a bounded area on Earth's surface using one or more closed rings of points. The outer ring defines the boundary; optional inner rings define excluded areas (holes, like courtyards or lakes within land).

A Polygon ring must have at least 4 points, with the first and last points identical to close the ring.

Use Polygons for areas and regions: park boundaries, property parcels, delivery zones, geofences, administrative boundaries.

## Creating a Simple Polygon

Create a Polygon with just an outer boundary (no holes):

```typescript
import { Point, LinearRing, Polygon } from '@cratis/fundamentals';

const park = new Polygon(
    new LinearRing([
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

```typescript
import { Point, LinearRing, Polygon } from '@cratis/fundamentals';

const buildingFootprint = new Polygon(
    new LinearRing([ // Outer boundary
        new Point(0, 0),
        new Point(20, 0),
        new Point(20, 20),
        new Point(0, 20),
        new Point(0, 0)
    ]),
    [
        new LinearRing([ // Courtyard (hole)
            new Point(5, 5),
            new Point(15, 5),
            new Point(15, 15),
            new Point(5, 15),
            new Point(5, 5)
        ])
    ]
);
```

## Using Polygons in Classes

Polygons work as properties in your classes:

```typescript
import { Polygon, field } from '@cratis/fundamentals';

class Zone {
    @field(String)
    name!: string;

    @field(String)
    type!: string;

    @field(Polygon)
    boundary!: Polygon;
}

const lot = new Zone();
lot.name = 'Main Parking';
lot.type = 'Parking';
lot.boundary = new Polygon(
    new LinearRing([
        new Point(10.7522, 59.9139),
        new Point(10.7622, 59.9139),
        new Point(10.7622, 59.9239),
        new Point(10.7522, 59.9239),
        new Point(10.7522, 59.9139)
    ]),
    []
);
```

## Accessing Shell and Holes

Access the outer boundary and any holes:

```typescript
import { Polygon } from '@cratis/fundamentals';

const polygon = new Polygon(
    new LinearRing([...]),
    [new LinearRing([...])]
);

const shell = polygon.shell;    // Outer boundary
const holes = polygon.holes;    // Array of holes
console.log(holes.length);      // Number of holes
```

## LinearRing

A `LinearRing` represents a closed boundary. It must have at least 4 points, with the first and last identical to close the ring:

```typescript
import { Point, LinearRing } from '@cratis/fundamentals';

const ring = new LinearRing([
    new Point(0, 0),
    new Point(10, 0),
    new Point(10, 10),
    new Point(0, 10),
    new Point(0, 0) // Closes the ring
]);
```

## Serialization

Polygons automatically serialize to GeoJSON format. See [Geospatial Serialization](../serialization/geospatial.md) for details on JSON handling.
