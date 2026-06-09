# LineString

`LineString` represents a line composed of two or more connected points. It provides GeoJSON-compliant automatic JSON serialization support, making it easy to work with paths and routes.

## Creating a LineString

Create a line string with multiple points:

```typescript
import { Point, LineString } from '@cratis/fundamentals';

const route = new LineString([
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);

console.log(route.coordinates.length); // 3
```

## JSON Serialization

The `JsonSerializer` automatically handles `LineString` serialization and deserialization following the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson).

### Serialization

```typescript
import { Point, LineString, JsonSerializer } from '@cratis/fundamentals';

const lineString = new LineString([
    new Point(10.5, 20.3),
    new Point(11.2, 21.1),
    new Point(12.0, 22.0)
]);
const json = JsonSerializer.serialize(lineString);

console.log(json);
// Output: {"type":"LineString","coordinates":[[10.5,20.3],[11.2,21.1],[12.0,22.0]]}
```

### Deserialization

```typescript
import { LineString, JsonSerializer } from '@cratis/fundamentals';

const json = '{"type":"LineString","coordinates":[[10.5,20.3],[11.2,21.1],[12.0,22.0]]}';
const lineString = JsonSerializer.deserialize(LineString, json);

console.log(lineString instanceof LineString); // true
console.log(lineString.coordinates.length);    // 3
console.log(lineString.coordinates[0].longitude); // 10.5
console.log(lineString.coordinates[0].latitude);  // 20.3
```

### Validation

The deserializer validates that:
- The `type` property is "LineString"
- The `coordinates` property is an array with at least two coordinate pairs
- Each coordinate pair is an array of two numbers [longitude, latitude]

If validation fails, an error will be thrown:

```typescript
import { LineString, JsonSerializer } from '@cratis/fundamentals';

const invalidJson = '{"type":"LineString","coordinates":[[10.5,20.3]]}'; // Only 1 point

try {
    const lineString = JsonSerializer.deserialize(LineString, invalidJson);
} catch (error) {
    console.log(error.message);
    // "Cannot deserialize LineString: invalid GeoJSON format"
}
```

## Using in Classes

LineString can be used as a property in your classes:

```typescript
import { LineString, field } from '@cratis/fundamentals';

class Route {
    @field(String)
    name!: string;

    @field(LineString)
    path!: LineString;
}
```

## Integration with Backend

Since both C# and TypeScript use the same GeoJSON format for `LineString`, data round-trips cleanly:

**C# Backend:**

```csharp
public record Route(string Name, LineString Path);
```

**TypeScript Frontend:**

```typescript
class Route {
    @field(String)
    name!: string;

    @field(LineString)
    path!: LineString;
}
```

Both will serialize to the same GeoJSON structure:

```json
{
    "name": "Mountain Trail",
    "path": {
        "type": "LineString",
        "coordinates": [
            [10.7522, 59.9139],
            [10.7625, 59.9245],
            [10.7728, 59.9351]
        ]
    }
}
```

## Use Cases

LineString is useful for:
- **Routes and paths**: Hiking trails, bike routes, driving directions
- **Boundaries**: Rivers, roads, property lines
- **Connections**: Network cables, pipeline routes
- **Trajectories**: Vehicle tracks, flight paths
