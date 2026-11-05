---
title: Cratis Fundamentals geospatial types in .NET
description: Cratis Fundamentals provides Point, LineString and Polygon records for .NET. Choose a shape for a location, route or area and serialize it as GeoJSON.
---
# Geospatial Types

Cratis Fundamentals provides C# geospatial records for representing a location, a connected path or a bounded area. Choose `Point`, `LineString` or `Polygon` according to what your application needs to describe. Their JSON converters use [GeoJSON](https://www.mongodb.com/docs/manual/reference/geojson/), a JSON format for geographic shapes.

## Types

| Type | Use for |
|------|---------|
| [Point](./point.md) | Single locations (addresses, landmarks, sensors) |
| [LineString](./linestring.md) | Connected paths and routes |
| [Polygon](./polygon.md) | Areas and regions bounded by rings |

<a id="key-features"></a>

## Using the types in a model

The records live in `Cratis.Geospatial`, so a model can declare whether a property holds a location, path or area. Each uses longitude before latitude. A `Polygon` uses a `LinearRing` for its outer boundary and for any holes inside it.

The GeoJSON converters are included in `Cratis.Json.Globals.JsonSerializerOptions`. If your code uses its own serializer options, register the converters there; choosing a record type alone does not enable GeoJSON output. [Geospatial Serialization](../serialization/geospatial.md) covers that setup.

Read [Understanding Geospatial Types](./understanding.md) to decide which shape fits your data, then follow the type links above for construction examples.
