# LineString

## What is a LineString?

A LineString represents a connected path of two or more geographic points, forming a line or route. Unlike a simple array of points, a LineString explicitly represents the path connecting consecutive points.

A LineString must have at least two Points. It represents the shortest path (geodesic) between each consecutive pair of points on Earth's surface.

Use LineStrings for routes, boundaries, and trajectories: hiking trails, river boundaries, road networks, vehicle movement history.

## Creating a LineString

Create a LineString by providing two or more Points:

```csharp
using Cratis.Geospatial;

var route = new LineString(
[
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);

Console.WriteLine($"Points in route: {route.Coordinates.Length}"); // 3
```

A LineString must have at least two Points. The points are stored in order, representing the path they form.

## Using LineStrings in Models

LineStrings work as properties in domain models:

```csharp
using Cratis.Geospatial;

public record Route(
    string Name,
    LineString Path
);

var trail = new Route(
    "Mountain Trail",
    new LineString(
    [
        new Point(10.7522, 59.9139), // Start
        new Point(10.7625, 59.9245), // Waypoint 1
        new Point(10.7728, 59.9351)  // End
    ])
);
```

## Accessing Coordinates

Once created, access the Points in a LineString:

```csharp
var route = new LineString(
[
    new Point(10.5, 20.3),
    new Point(11.2, 21.1)
]);

foreach (var point in route.Coordinates)
{
    Console.WriteLine($"({point.Longitude}, {point.Latitude})");
}

var firstPoint = route.Coordinates[0];
var lastPoint = route.Coordinates[^1];
```

## Serialization

LineStrings automatically serialize to GeoJSON format. See [Geospatial Serialization](../serialization/geospatial.md) for details on JSON handling.
