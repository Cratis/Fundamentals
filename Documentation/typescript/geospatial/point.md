# Work with Points

## Creating a Point

Create a Point with longitude and latitude:

```typescript
import { Point } from '@cratis/fundamentals';

const location = new Point(10.5, 20.3);

console.log(location.longitude); // 10.5
console.log(location.latitude);  // 20.3
```

The first parameter is **longitude** (east-west position), the second is **latitude** (north-south position).

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

## Serializing and Deserializing

The `JsonSerializer` automatically handles Point serialization and deserialization following GeoJSON format:

```typescript
import { Point, JsonSerializer } from '@cratis/fundamentals';

const point = new Point(10.5, 20.3);
const json = JsonSerializer.serialize(point);
// Output: {"type":"Point","coordinates":[10.5,20.3]}

const deserialized = JsonSerializer.deserialize(Point, json);
```

## Validation

The deserializer validates the GeoJSON structure:
- The `type` property must be "Point"
- The `coordinates` property must be an array with exactly two numbers [longitude, latitude]

If validation fails, an error is thrown. Ensure your incoming data has the correct structure before deserialization.

## Working with Coordinates

Access and work with a Point's coordinates:

```typescript
import { Point } from '@cratis/fundamentals';

const location = new Point(10.5, 20.3);
const longPart = location.longitude;
const latPart = location.latitude;

// Create new Point from existing
const adjusted = new Point(longPart + 0.1, latPart + 0.1);
```

## Deprecated Coordinate Type

If you have existing code using the deprecated `Coordinate` type, replace it with `Point`:

```typescript
// Old
const coord = new Coordinate(10.5, 20.3);

// New
const point = new Point(10.5, 20.3);
```
