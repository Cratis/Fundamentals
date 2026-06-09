# Point

## What is a Point?

A Point represents a single geographic location on Earth's surface defined by a longitude and latitude pair.

- **Longitude** (east-west position): ranges from -180° to 180°
- **Latitude** (north-south position): ranges from -90° to 90°

Use Points to represent discrete locations like addresses, landmarks, device positions, or any fixed geographic location.

## Creating a Point

Create a point with longitude and latitude:

```typescript
import { Point } from '@cratis/fundamentals';

const location = new Point(10.5, 20.3);

console.log(location.longitude); // 10.5
console.log(location.latitude);  // 20.3
```

The first parameter is **longitude**, the second is **latitude**.

## Using Points in Classes

Points work as properties in your classes:

```typescript
import { Point, field } from '@cratis/fundamentals';

class Location {
    @field(String)
    name!: string;

    @field(Point)
    position!: Point;
}

const office = new Location();
office.name = 'Headquarters';
office.position = new Point(10.7522, 59.9139); // Oslo, Norway
```

## Accessing Coordinates

Access and work with a Point's coordinates:

```typescript
import { Point } from '@cratis/fundamentals';

const location = new Point(10.5, 20.3);
const longPart = location.longitude;
const latPart = location.latitude;

// Create new Point from existing
const adjusted = new Point(longPart + 0.1, latPart + 0.1);
```

## Serialization

Points automatically serialize to GeoJSON format. See [Geospatial Serialization](../serialization/geospatial.md) for details on JSON handling.
