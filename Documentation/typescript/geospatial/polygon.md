# Work with Polygons

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

Create a Polygon with excluded areas (holes):

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

## Serializing and Deserializing

The `JsonSerializer` automatically handles Polygon serialization and deserialization:

```typescript
import { Point, LinearRing, Polygon, JsonSerializer } from '@cratis/fundamentals';

const polygon = new Polygon(
    new LinearRing([
        new Point(0, 0),
        new Point(10, 0),
        new Point(10, 10),
        new Point(0, 10),
        new Point(0, 0)
    ]),
    []
);
const json = JsonSerializer.serialize(polygon);
// Output: {"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}

const deserialized = JsonSerializer.deserialize(Polygon, json);
```

## Validation

The deserializer validates:
- The `type` property is "Polygon"
- The `coordinates` property is an array with at least one ring (the shell)
- Each ring has coordinate pairs
- Each coordinate pair is an array of two numbers [longitude, latitude]

If validation fails, an error is thrown.

## Accessing Shell and Holes

Once created, you can access the shell and holes:

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

## LinearRing Helper

A `LinearRing` represents a closed boundary. Convert it to GeoJSON format when needed:

```typescript
import { Point, LinearRing } from '@cratis/fundamentals';

const ring = new LinearRing([
    new Point(0, 0),
    new Point(10, 0),
    new Point(10, 10),
    new Point(0, 10),
    new Point(0, 0)
]);

const geoJsonArray = ring.toJSON();
// Output: [[0,0],[10,0],[10,10],[0,10],[0,0]]
```
