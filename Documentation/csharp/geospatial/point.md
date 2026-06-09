# Point

## What is a Point?

A Point represents a single geographic location on Earth's surface defined by a longitude and latitude pair.

- **Longitude** (east-west position): ranges from -180° to 180°
- **Latitude** (north-south position): ranges from -90° to 90°

Use Points to represent discrete locations like addresses, landmarks, device positions, or any fixed geographic location.

## Creating a Point

Create a Point with longitude and latitude:

```csharp
using Cratis.Geospatial;

var location = new Point(10.5, 20.3);
Console.WriteLine($"Longitude: {location.Longitude}"); // 10.5
Console.WriteLine($"Latitude: {location.Latitude}");   // 20.3
```

The first parameter is **longitude**, the second is **latitude**.

## Using Points in Models

Points work as properties in domain models and records:

```csharp
using Cratis.Geospatial;

public record Location(
    string Name,
    Point Position
);

var office = new Location(
    "Headquarters",
    new Point(10.7522, 59.9139) // Oslo, Norway
);
```

## Accessing Coordinates

Once you have a Point, access its coordinates:

```csharp
var point = new Point(10.5, 20.3);

var lng = point.Longitude;
var lat = point.Latitude;

// Create a new Point from existing coordinates
var adjusted = new Point(lng + 0.1, lat + 0.1);
```

## Serialization

Points automatically serialize to GeoJSON format. See [Geospatial Serialization](../serialization/geospatial.md) for details on JSON handling.
