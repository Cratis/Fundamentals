# Coordinate (Deprecated)

> **⚠️ Deprecated**: Use `Point` from the `geospatial` module instead. This class is kept for backward compatibility and will be removed in a future version.

`Coordinate` is now an alias for `Point` that maintains the old JSON serialization format for backward compatibility. For new code, use `Point` which follows the GeoJSON specification.

See [Point](./geospatial_point.md) for current documentation.

## Backward Compatibility

The deprecated `Coordinate` class extends `Point` but overrides `toJSON()` to maintain the old format:

```typescript
// Coordinate toJSON() output (old format)
{
    "longitude": 10.5,
    "latitude": 20.3
}

// Point toJSON() output (new GeoJSON format)
{
    "type": "Point",
    "coordinates": [10.5, 20.3]
}
```

When using `JsonSerializer`:
- `JsonSerializer.serialize(coordinate)` where `coordinate` is a `Coordinate` instance will output the old format
- `JsonSerializer.deserialize(Coordinate, json)` expects the old format with `longitude` and `latitude` properties
- `JsonSerializer.serialize(point)` where `point` is a `Point` instance will output GeoJSON format
- `JsonSerializer.deserialize(Point, json)` expects GeoJSON format with `type` and `coordinates`

## Migration

Replace:
```typescript
import { Coordinate } from '@cratis/fundamentals';
const location = new Coordinate(10.5, 20.3);
```

With:
```typescript
import { Point } from '@cratis/fundamentals';
const location = new Point(10.5, 20.3);
```

**Important**: If you have existing JSON data in the old format, you will need to migrate it to GeoJSON format or continue using `Coordinate` until your data is migrated.
