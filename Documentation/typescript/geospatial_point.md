# Point

`Point` represents a geographic location with longitude and latitude values. It provides GeoJSON-compliant automatic JSON serialization support, making it easy to work with geospatial data.

## Creating a Point

Create a point with longitude and latitude:

```typescript
import { Point } from '@cratis/fundamentals';

const location = new Point(10.5, 20.3);

console.log(location.longitude); // 10.5
console.log(location.latitude);  // 20.3
```

## JSON Serialization

The `JsonSerializer` automatically handles `Point` serialization and deserialization following the [GeoJSON specification](https://www.mongodb.com/docs/manual/reference/geojson/#std-label-geospatial-indexes-store-geojson).

### Serialization

```typescript
import { Point, JsonSerializer } from '@cratis/fundamentals';

const point = new Point(10.5, 20.3);
const json = JsonSerializer.serialize(point);

console.log(json);
// Output: {"type":"Point","coordinates":[10.5,20.3]}
```

### Deserialization

```typescript
import { Point, JsonSerializer } from '@cratis/fundamentals';

const json = '{"type":"Point","coordinates":[10.5,20.3]}';
const point = JsonSerializer.deserialize(Point, json);

console.log(point instanceof Point); // true
console.log(point.longitude);        // 10.5
console.log(point.latitude);         // 20.3
```

### Validation

The deserializer validates that:
- The `type` property is "Point"
- The `coordinates` property is an array with exactly two numbers [longitude, latitude]

If validation fails, an error will be thrown:

```typescript
import { Point, JsonSerializer } from '@cratis/fundamentals';

const invalidJson = '{"type":"Point","coordinates":[10.5]}';

try {
    const point = JsonSerializer.deserialize(Point, invalidJson);
} catch (error) {
    console.log(error.message);
    // "Cannot deserialize Point: invalid GeoJSON format"
}
```

## Using in Classes

Point can be used as a property in your classes:

```typescript
import { Point, field } from '@cratis/fundamentals';

class Location {
    @field(String)
    name!: string;

    @field(Point)
    position!: Point;
}
```

## Integration with Backend

Since both C# and TypeScript use the same GeoJSON format for `Point`, data round-trips cleanly:

**C# Backend:**
```csharp
public record Location(string Name, Point Position);
```

**TypeScript Frontend:**
```typescript
class Location {
    @field(String)
    name!: string;

    @field(Point)
    position!: Point;
}
```

Both will serialize to the same GeoJSON structure:

```json
{
    "name": "Headquarters",
    "position": {
        "type": "Point",
        "coordinates": [10.7522, 59.9139]
    }
}
```

## Migration from Coordinate

If you have existing code using `Coordinate`, update it to use `Point`. Note the JSON format change:

**Old format (Coordinate):**
```json
{
    "longitude": 10.5,
    "latitude": 20.3
}
```

**New format (Point):**
```json
{
    "type": "Point",
    "coordinates": [10.5, 20.3]
}
```

The `Coordinate` class is now deprecated but kept for backward compatibility. It extends `Point` but overrides `toJSON()` to maintain the old format.
