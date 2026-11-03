---
description: Serialize TypeScript Point, LineString and Polygon objects to and from GeoJSON with JsonSerializer and no extra configuration.
---
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

**GeoJSON format and implemented checks:**

- GeoJSON requires `type` "Point" and `coordinates` as two numbers [longitude, latitude]
- The reader checks the `type` value, that `coordinates` is present, and that its length is two. It does not check that the elements are numbers

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

**GeoJSON format and implemented checks:**

- GeoJSON requires `type` "LineString" and `coordinates` as an array of at least two [longitude, latitude] pairs
- The reader checks the `type` value, that `coordinates` is present, and that it has at least two points
- It does not check the array type, that each pair is an array of two numbers, or each pair's length

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

**GeoJSON format and implemented checks:**

- GeoJSON requires `type` "Polygon" and `coordinates` as one or more closed rings of [longitude, latitude] pairs
- The reader checks the `type` value and that `coordinates` is present and non-empty
- Each `LinearRing` requires at least four points; ring closure is not checked
- It does not check that each coordinate is an array of two numbers or each pair's length

## Validation Errors

If a reader check above fails, an error is thrown. The example below fails the Point length check:

```typescript
try {
    const json = '{"type":"Point","coordinates":[10.5]}'; // Missing latitude
    const point = JsonSerializer.deserialize(Point, json);
} catch (error) {
    console.log(error.message); // Cannot deserialize Point: invalid GeoJSON format
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
