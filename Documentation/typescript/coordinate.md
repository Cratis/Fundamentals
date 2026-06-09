# Coordinate (Deprecated)

> **⚠️ Deprecated**: Use `Point` from the `geospatial` module instead. This class is kept for backward compatibility and will be removed in a future version.

`Coordinate` is now an alias for `Point`. For new code, use `Point` which follows the GeoJSON specification.

See [Point](./geospatial_point.md) for current documentation.

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

Note that `Coordinate` serializes to the old format `{"longitude":10.5,"latitude":20.3}`, while `Point` uses GeoJSON format `{"type":"Point","coordinates":[10.5,20.3]}`.
