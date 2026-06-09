# Understanding Geospatial Types

Geospatial types represent locations and areas on Earth's surface. Cratis provides three core types, each suited for different scenarios.

## Point

A **Point** is the simplest geospatial type — a single location defined by longitude and latitude coordinates. Use Points when you need to store or track a discrete location: a warehouse address, a vehicle's current position, a user's home location.

Points are always paired: longitude (east-west, -180° to 180°) and latitude (north-south, -90° to 90°).

## LineString

A **LineString** is a path formed by connecting two or more Points in order. Unlike an array of points, a LineString explicitly represents the connections between them, forming a continuous line. Use LineStrings for routes, boundaries, and trajectories: a hiking trail, a river boundary, a vehicle's movement history.

A LineString must have at least two points. It represents the shortest path (geodesic) between consecutive pairs of points on Earth's surface.

## Polygon

A **Polygon** encloses an area using one or more closed rings of points. The outer ring is the boundary; optional inner rings define excluded areas (like courtyards or lakes within land). Use Polygons for areas and regions: park boundaries, property parcels, delivery zones, geofences.

A Polygon ring must have at least 4 points, with the first and last points identical to close it.

## When to Use Each

| Scenario | Type |
|----------|------|
| Store a single location (address, landmark, sensor) | Point |
| Track position over time | Point (repeated snapshots) |
| Represent a route or boundary line | LineString |
| Represent a bounded area or region | Polygon |
| Geofencing (location triggers) | Polygon |
| Administrative boundaries | Polygon |

## JSON Serialization

All types serialize to [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/) format — a standard adopted by geographic databases and mapping services. This ensures your data integrates seamlessly with external tools and services.

When you send geospatial types to a server or store them, they automatically convert to GeoJSON. The same conversion happens in reverse when deserializing.

**Point example:**
```json
{
    "type": "Point",
    "coordinates": [10.5, 20.3]
}
```

**LineString example:**
```json
{
    "type": "LineString",
    "coordinates": [[10.5, 20.3], [11.2, 21.1], [12.0, 22.0]]
}
```

**Polygon example:**
```json
{
    "type": "Polygon",
    "coordinates": [[[0, 0], [10, 0], [10, 10], [0, 10], [0, 0]]]
}
```

## Full-Stack Type Safety

Use the same geospatial types on both C# backend and TypeScript frontend. Data round-trips cleanly in GeoJSON format:

```csharp
// C# backend
public record Location(string Name, Point Position);
```

```typescript
// TypeScript frontend
class Location {
    @field(String)
    name!: string;

    @field(Point)
    position!: Point;
}
```

Both serialize and deserialize to identical GeoJSON, so the type information flows end-to-end.
