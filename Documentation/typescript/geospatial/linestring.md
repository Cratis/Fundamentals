# LineString

## What is a LineString?

A LineString represents a connected path of two or more geographic points, forming a line or route. Unlike a simple array of points, a LineString explicitly represents the path connecting consecutive points.

A LineString must have at least two Points. It represents the shortest path (geodesic) between each consecutive pair of points on Earth's surface.

Use LineStrings for routes, boundaries, and trajectories: hiking trails, river boundaries, road networks, vehicle movement history.

## Creating a LineString

Create a LineString by providing two or more Points:

```typescript
import { Point, LineString } from '@cratis/fundamentals';

const route = new LineString([
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);

console.log(route.coordinates.length); // 3
```

A LineString must have at least two Points. The points are stored in order, representing the path they form.

## Using LineStrings in Classes

LineStrings work as properties in your classes:

```typescript
import { Point, LineString, field } from '@cratis/fundamentals';

class Route {
    @field(String)
    name!: string;

    @field(LineString)
    path!: LineString;
}

const trail = new Route();
trail.name = 'Mountain Trail';
trail.path = new LineString([
    new Point(10.7522, 59.9139), // Start
    new Point(10.7625, 59.9245), // Waypoint 1
    new Point(10.7728, 59.9351)  // End
]);
```

## Serialization

LineStrings automatically serialize to GeoJSON format. See [Geospatial Serialization](../serialization/geospatial.md) for details on JSON handling.

## Accessing Coordinates

Once created, you can access the Points in a LineString:

```typescript
import { LineString } from '@cratis/fundamentals';

const route = new LineString([
    new Point(10.5, 20.3),
    new Point(11.2, 21.1)
]);

for (const point of route.coordinates) {
    console.log(`(${point.longitude}, ${point.latitude})`);
}
```
