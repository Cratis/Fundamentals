---
title: TypeScript JsonSerializer for GeoJSON geometry
description: JsonSerializer converts TypeScript Point, LineString, and Polygon instances to GeoJSON and back, with built-in converters and limited input checks.
---
<a id="geospatial-serialization"></a>

`JsonSerializer` converts TypeScript `Point`, `LineString`, and `Polygon` instances to [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/), a JSON format for geographic data, and reads them back into geometry classes. Use it when sending a location or boundary as JSON and you need geometry instances on receipt. Coordinates use longitude first, then latitude.

## Automatic Serialization

Cratis Fundamentals registers the geometry converters for you. Import the geometry constructor and `JsonSerializer`, then pass the constructor when reading JSON. This example reads the point back with longitude `10.5` and latitude `20.3`. The later excerpts also use `LineString`, `Polygon`, `LinearRing`, and `field` from the same package.

```typescript
import { Point, JsonSerializer } from '@cratis/fundamentals';

const point = new Point(10.5, 20.3);
const json = JsonSerializer.serialize(point);
// Output: {"type":"Point","coordinates":[10.5,20.3]}

const deserialized = JsonSerializer.deserialize(Point, json);
```

## Point Serialization

A `Point` writes a `type` of `"Point"` and a single coordinate pair. The same pair supplies the constructor arguments when you deserialize it.

```typescript
const point = new Point(10.5, 20.3);
const json = JsonSerializer.serialize(point);
// {"type":"Point","coordinates":[10.5,20.3]}

const deserialized = JsonSerializer.deserialize(Point, json);
```

Provide two numbers in `[longitude, latitude]` order. The reader checks that `type` is `"Point"`, that `coordinates` is present, and that its length is two. It does not check that the elements are numbers, so successful deserialization alone does not establish valid coordinate values.

## LineString Serialization

Use a `LineString` for an ordered sequence of at least two points. Its constructor rejects shorter sequences, and serialization writes each point as a coordinate pair.

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

The GeoJSON input must contain an array of at least two `[longitude, latitude]` pairs. The reader checks `type`, the presence of `coordinates`, and a minimum length of two. It then maps the coordinates to points without first validating that the container is an array or that each pair contains exactly two numbers.

## Polygon Serialization

A `Polygon` has an outer boundary, called its shell, and optional inner boundaries for holes. Each boundary is a `LinearRing`. Serialization puts the shell first in the GeoJSON `coordinates` array, followed by the holes.

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

A polygon with one hole has two coordinate rings:

```json
{
    "type": "Polygon",
    "coordinates": [
        [[0,0],[20,0],[20,20],[0,20],[0,0]],
        [[5,5],[15,5],[15,15],[5,15],[5,5]]
    ]
}
```

Supply one or more closed rings of `[longitude, latitude]` pairs. The reader checks that `type` is `"Polygon"` and that `coordinates` is present and non-empty. Constructing each `LinearRing` checks for at least four points, but does not check that the first and last points match. Repeat the first point yourself, as in both examples.

The reader also does not check that each coordinate is an array of exactly two numbers. Validate incoming geometry separately if you need to reject malformed coordinates or unclosed rings.

## Validation Errors

The reader throws when one of its checks fails. This point is missing its latitude, so it fails the length check before a `Point` can be constructed:

```typescript
try {
    const json = '{"type":"Point","coordinates":[10.5]}'; // Missing latitude
    const point = JsonSerializer.deserialize(Point, json);
} catch (error) {
    console.log(error.message); // Cannot deserialize Point: invalid GeoJSON format
}
```

## Full-Stack Serialization

The .NET `PointJsonConverter` writes the same `type` and coordinate pair that the TypeScript reader expects. For a point inside a TypeScript model, declare the field as `@field(Point)` so `JsonSerializer` uses its point converter when reading the containing object.

These excerpts show the matching model fields. The C# payload shown assumes the .NET point converter and camel-case property naming are configured.

C# model:

```csharp
public record Location(string Name, Point Position);

// Serializes to {"name":"...","position":{"type":"Point","coordinates":[...]}}
```

TypeScript model:

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

Use `LinearRing.toJSON()` when you need just the boundary's coordinate array. A ring is part of a polygon, not a separately registered geometry converter; its `toJSON()` result has no GeoJSON `type` wrapper.

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

To put geometry inside a larger model, continue with the [field decorator guide](field_decorator.md). The [JsonSerializer guide](json_serializer.md) covers nested models and custom converters for other payload shapes.
