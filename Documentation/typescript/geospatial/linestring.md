# Work with LineStrings

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

## Serializing and Deserializing

The `JsonSerializer` automatically handles LineString serialization and deserialization:

```typescript
import { Point, LineString, JsonSerializer } from '@cratis/fundamentals';

const lineString = new LineString([
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);
const json = JsonSerializer.serialize(lineString);
// Output: {"type":"LineString","coordinates":[[10.5,20.3],[11.2,21.1],[12.0,22.0]]}

const deserialized = JsonSerializer.deserialize(LineString, json);
```

## Validation

The deserializer validates:
- The `type` property is "LineString"
- The `coordinates` property is an array with at least two coordinate pairs
- Each coordinate pair is an array of two numbers [longitude, latitude]

If validation fails, an error is thrown.

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
