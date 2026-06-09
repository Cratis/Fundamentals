# Polygon

`Polygon` represents a bounded area on Earth's surface with an outer boundary (shell) and optional inner boundaries (holes) following the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson).

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

## Creating a Polygon

Create a simple polygon with just a shell (no holes):

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

Create a polygon with holes:

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

## JSON Serialization

The `JsonSerializer` automatically handles `Polygon` serialization and deserialization following the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson).

### Serialization

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

console.log(json);
// Output: {"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}
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

```typescript
import { Polygon, JsonSerializer } from '@cratis/fundamentals';

const json = '{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}';
const polygon = JsonSerializer.deserialize(Polygon, json);

console.log(polygon instanceof Polygon);       // true
console.log(polygon.shell.coordinates.length); // 5
console.log(polygon.holes.length);             // 0
```

### Validation

The deserializer validates that:
- The `type` property is "Polygon"
- The `coordinates` property is an array with at least one ring (the shell)
- Each ring has coordinate pairs
- Each coordinate pair is an array of two numbers [longitude, latitude]

If validation fails, an error will be thrown:

```typescript
import { Polygon, JsonSerializer } from '@cratis/fundamentals';

const invalidJson = '{"type":"Polygon","coordinates":[]}'; // No rings

try {
    const polygon = JsonSerializer.deserialize(Polygon, invalidJson);
} catch (error) {
    console.log(error.message);
    // "Cannot deserialize Polygon: invalid GeoJSON format"
}
```

## Using in Classes

Polygon can be used as a property in your classes:

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
```

## Integration with Backend

Since both C# and TypeScript use the same GeoJSON format for `Polygon`, data round-trips cleanly:

**C# Backend:**

```csharp
public record Zone(string Name, string Type, Polygon Boundary);
```

**TypeScript Frontend:**

```typescript
class Zone {
    @field(String)
    name!: string;

    @field(String)
    type!: string;

    @field(Polygon)
    boundary!: Polygon;
}
```

Both will serialize to the same GeoJSON structure:

```json
{
    "name": "Main Parking",
    "type": "Parking",
    "boundary": {
        "type": "Polygon",
        "coordinates": [
            [[10.7522,59.9139],[10.7622,59.9139],[10.7622,59.9239],[10.7522,59.9239],[10.7522,59.9139]]
        ]
    }
}
```

## LinearRing

A `LinearRing` is a closed line that forms the boundary of a polygon. The first and last points should be identical to close the ring, though this is not strictly enforced at creation time (it's a GeoJSON convention).

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

LinearRing has a `toJSON()` method that returns a GeoJSON coordinate array:

```typescript
console.log(ring.toJSON());
// Output: [[0,0],[10,0],[10,10],[0,10],[0,0]]
```
