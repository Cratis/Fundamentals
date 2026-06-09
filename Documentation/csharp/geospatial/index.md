# Geospatial Types

Cratis provides strongly-typed geospatial types for working with geographic coordinates following the [GeoJSON specification](https://geojson.org).

## Types

| Type | Use for |
|------|---------|
| [Point](./point.md) | Single locations (addresses, landmarks, sensors) |
| [LineString](./linestring.md) | Connected paths and routes |
| [Polygon](./polygon.md) | Areas and regions bounded by rings |

## Key Features

- **GeoJSON Compatible** — Standard format for geographic databases and mapping tools
- **Automatic Serialization** — Handled by the Cratis Application Model
- **Type Safe** — Compile-time validation through C# records
- **Full-Stack** — Use the same types on C# and TypeScript with automatic round-tripping

See [Understanding Geospatial Types](./understanding.md) for concepts and when to use each type.
