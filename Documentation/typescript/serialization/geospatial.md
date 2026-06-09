# Geospatial Serialization

Geospatial types (Point, LineString, Polygon) serialize to [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/) format — the standard representation for geographic data in JSON.

## Automatic Serialization

The `JsonSerializer` automatically handles geospatial serialization and deserialization following GeoJSON format. No manual configuration needed.

```typescript
import { Point, JsonSerializer } from '@cratis/fundamentals';

const point = new Point(10.5, 20.3);
const json = JsonSerializer.serialize(point);
// Output: {"type":"Point","coordinates":[10.5,20.3]}

const deserialized = JsonSerializer.deserialize(Point, json);
```

## Point Serialization

Points serialize to GeoJSON Point format:

```typescript
const point = new Point(10.5, 20.3);
const json = JsonSerializer.serialize(point);
// {"type":"Point","coordinates":[10.5,20.3]}

const deserialized = JsonSerializer.deserialize(Point, json);
```

**Validation:**
- `type` must be "Point"
- `coordinates` must be an array with exactly two numbers [longitude, latitude]

## LineString Serialization

LineStrings serialize to GeoJSON LineString format:

```typescript
const lineString = new LineString([
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);
const json = JsonSerializer.serialize(lineString);
// {"type":"LineString","coordinates":[[10.5,20.3],[11.2,21.1],[12.0,22.0]]}

const deserialized = JsonSerializer.deserialize(LineString, json);
```

**Validation:**
- `type` must be "LineString"
- `coordinates` must be an array with at least two coordinate pairs
- Each pair must be an array of two numbers [longitude, latitude]

## Polygon Serialization

Polygons serialize to GeoJSON Polygon format with optional holes:

```typescript
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
// {"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}

const deserialized = JsonSerializer.deserialize(Polygon, json);
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
- Each ring must have coordinate pairs
- Each coordinate must be an array of two numbers [longitude, latitude]

## Validation Errors

If deserialization fails validation, an error is thrown:

```typescript
try {
    const json = '{"type":"Point","coordinates":[10.5]}'; // Missing latitude
    const point = JsonSerializer.deserialize(Point, json);
} catch (error) {
    console.log(error.message); // Validation failed
}
```

## Full-Stack Serialization

C# and TypeScript use the same GeoJSON format, so data round-trips cleanly:

**C# Backend:**
```csharp
public record Location(string Name, Point Position);

// Serializes to {"name":"...","position":{"type":"Point","coordinates":[...]}}
```

**TypeScript Frontend:**
```typescript
class Location {
    @field(String)
    name!: string;

    @field(Point)
    position!: Point;
}

// Deserializes from the same GeoJSON structure
```

## LinearRing Serialization

LinearRing objects serialize to coordinate arrays:

```typescript
const ring = new LinearRing([
    new Point(0, 0),
    new Point(10, 0),
    new Point(10, 10),
    new Point(0, 10),
    new Point(0, 0)
]);

const array = ring.toJSON();
// Output: [[0,0],[10,0],[10,10],[0,10],[0,0]]
```
