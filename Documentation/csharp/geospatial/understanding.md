---
title: Cratis Fundamentals geospatial types — choosing a shape
description: Use Point for a location, LineString for a connected path and Polygon for a bounded area in .NET. Learn coordinate order and GeoJSON representation.
---
Use `Point` for a single location, `LineString` for a connected path and `Polygon` for an area with a boundary. In Cratis Fundamentals, these geospatial types let a model distinguish a delivery vehicle's current position from its route and the delivery zone it serves.

## Point

A `Point` is a single location defined by longitude and latitude. Use it for the coordinates of a warehouse or a vehicle's current position. A history of positions can remain a collection of Points when you need separate observations rather than a path connecting them.

Coordinate order matters: longitude comes first, then latitude. Longitude measures east-west position from -180° to 180°; latitude measures north-south position from -90° to 90°. Reversing the pair describes a different location.

## LineString

A `LineString` describes a path connecting two or more Points in order. Choose it when the connections matter, such as a delivery route or a hiking trail. The same positions stored as separate observations do not express that connected path.

A LineString needs at least two points. It stores the path's coordinates; it does not calculate a route or the distance between positions.

## Polygon

A `Polygon` describes an area enclosed by an outer ring of points, called the shell. Optional inner rings describe holes excluded from the area, such as a courtyard within a building footprint. Use a Polygon for a delivery zone when the area inside the boundary matters; use a LineString when you only need a boundary line.

Each ring is a `LinearRing` with at least four points, including a repeated first point at the end to close it. These C# records store the coordinates without validating them in their constructors, so supply valid coordinate ranges, point counts and closed rings when creating them.

## When to Use Each

| Scenario | Type |
|----------|------|
| Store a single location (address, landmark, sensor) | Point |
| Track position over time | Point (repeated snapshots) |
| Represent a route or boundary line | LineString |
| Represent a bounded area or region | Polygon |
| Describe a geofence boundary | Polygon |
| Represent an administrative area | Polygon |

A Polygon represents the zone used for geofencing. Your application or a spatial database still needs to check whether a position falls inside it and decide when to trigger an action.

## JSON Serialization

The converters serialize these types as [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/) geometry objects. Each object has a `type` that names the shape and `coordinates` whose nesting reflects its structure.

The converters are registered in the Fundamentals global serializer options (`Cratis.Json.Globals.JsonSerializerOptions`). Code that serializes with its own options must register the converters itself. They also read GeoJSON back into the corresponding records. Follow [Geospatial Serialization](../serialization/geospatial.md) for the registration details.

A Point has one longitude-latitude pair. In this example, longitude is 10.5 and latitude is 20.3:

```json
{
    "type": "Point",
    "coordinates": [10.5, 20.3]
}
```

A LineString has an ordered array of coordinate pairs:

```json
{
    "type": "LineString",
    "coordinates": [[10.5, 20.3], [11.2, 21.1], [12.0, 22.0]]
}
```

A Polygon adds another array level to group its rings. The first ring is the shell; any following rings are holes. This example has only a shell, with its first coordinate repeated at the end:

```json
{
    "type": "Polygon",
    "coordinates": [[[0, 0], [10, 0], [10, 10], [0, 10], [0, 0]]]
}
```

<a id="full-stack-type-safety"></a>

## Sharing a Point with TypeScript

Cratis Fundamentals also provides TypeScript classes for these shapes. A C# model can declare a Point property:

```csharp
// C# backend
public record Location(string Name, Point Position);
```

The corresponding TypeScript model declares the property with `@field(Point)`, which tells the Fundamentals JSON serializer to deserialize it as a Point:

```typescript
// TypeScript frontend
class Location {
    @field(String)
    name!: string;

    @field(Point)
    position!: Point;
}
```

With the Fundamentals serializers, the Point value uses the GeoJSON representation shown above. Plain `JSON.parse` returns an object, not a Point instance; use the TypeScript serializer when you need the typed value.

Once you have chosen a shape, follow the [Point](./point.md), [LineString](./linestring.md) or [Polygon](./polygon.md) examples to add it to a C# model.
