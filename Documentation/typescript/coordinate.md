# Coordinate

`Coordinate` represents a geographic location with longitude and latitude values. It provides automatic JSON serialization support, making it easy to work with geospatial data.

## Creating a Coordinate

Create a coordinate with longitude and latitude:

```typescript
import { Coordinate } from '@cratis/fundamentals';

const location = new Coordinate(10.5, 20.3);

console.log(location.longitude); // 10.5
console.log(location.latitude);  // 20.3
```

## JSON Serialization

The `JsonSerializer` automatically handles `Coordinate` serialization and deserialization.

### Serialization

```typescript
import { Coordinate, JsonSerializer } from '@cratis/fundamentals';

const coordinate = new Coordinate(10.5, 20.3);
const json = JsonSerializer.serialize(coordinate);

console.log(json);
// Output: {"longitude":10.5,"latitude":20.3}
```

### Deserialization

```typescript
import { Coordinate, JsonSerializer } from '@cratis/fundamentals';

const json = '{"longitude":10.5,"latitude":20.3}';
const coordinate = JsonSerializer.deserialize(Coordinate, json);

console.log(coordinate instanceof Coordinate); // true
console.log(coordinate.longitude);             // 10.5
console.log(coordinate.latitude);              // 20.3
```

### Validation

The deserializer validates that both `longitude` and `latitude` properties are present in the JSON. If either property is missing, an error will be thrown:

```typescript
import { Coordinate, JsonSerializer } from '@cratis/fundamentals';

const invalidJson = '{"longitude":10.5}';

try {
    const coordinate = JsonSerializer.deserialize(Coordinate, invalidJson);
} catch (error) {
    console.log(error.message);
    // "Cannot deserialize Coordinate: longitude and latitude are required"
}
```

## Using in Classes

Coordinate can be used as a property in your classes with the `@field` decorator:

```typescript
import { Coordinate } from '@cratis/fundamentals';
import { field } from '@cratis/fundamentals';

class Location {
    @field(String)
    name!: string;

    @field(Coordinate)
    position!: Coordinate;
}
```

## Null Handling

When serializing a null coordinate, the serializer returns `null` rather than creating a default coordinate:

```typescript
import { JsonSerializer } from '@cratis/fundamentals';

class OptionalLocation {
    @field(Coordinate)
    position?: Coordinate;
}

const location = new OptionalLocation();
const json = JsonSerializer.serialize(location);
// The position property will be null or omitted
```

## Integration with Backend

Since both C# and TypeScript use the same JSON format for `Coordinate`, data round-trips cleanly:

**C# Backend:**
```csharp
public record Location(string Name, Coordinate Position);
```

**TypeScript Frontend:**
```typescript
class Location {
    @field(String)
    name!: string;

    @field(Coordinate)
    position!: Coordinate;
}
```

Both will serialize to the same JSON structure:

```json
{
    "name": "Headquarters",
    "position": {
        "longitude": 10.7522,
        "latitude": 59.9139
    }
}
```
