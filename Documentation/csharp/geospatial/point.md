# Work with Points

## Creating a Point

Create a Point with longitude and latitude:

```csharp
using Cratis.Geospatial;

var location = new Point(10.5, 20.3);
Console.WriteLine($"Longitude: {location.Longitude}"); // 10.5
Console.WriteLine($"Latitude: {location.Latitude}");   // 20.3
```

The first parameter is **longitude** (east-west position), the second is **latitude** (north-south position).

## Using Points in Models

Points work as properties in domain models and records:

```csharp
using Cratis.Geospatial;

public record Location(
    string Name,
    Point Position
);

var office = new Location(
    "Headquarters",
    new Point(10.7522, 59.9139) // Oslo, Norway
);
```

## Serializing and Deserializing

The `PointJsonConverter` is automatically registered when you use the Cratis Application Model. You can serialize and deserialize Points directly with `JsonSerializer`:

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var point = new Point(10.5, 20.3);
var json = JsonSerializer.Serialize(point, Globals.JsonSerializerOptions);
// Output: {"type":"Point","coordinates":[10.5,20.3]}

var deserialized = JsonSerializer.Deserialize<Point>(json, Globals.JsonSerializerOptions);
```

## Manual Converter Configuration

If you need to configure the converter manually (outside the Application Model):

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new PointJsonConverter()
    }
};

var point = new Point(10.5, 20.3);
var json = JsonSerializer.Serialize(point, options);
```

## Validation

The converter validates the GeoJSON structure:
- The `type` property must be "Point"
- The `coordinates` property must be an array with exactly two numbers [longitude, latitude]

If validation fails, a `JsonException` is thrown. Ensure your incoming data has the correct structure before deserialization.

## Migration from Coordinate

If you have existing code using the deprecated `Coordinate` type, rename all references to `Point`:

```csharp
// Old
var coord = new Coordinate(10.5, 20.3);

// New
var point = new Point(10.5, 20.3);
```

Update any stored JSON from the old format to GeoJSON format:

```json
// Old format
{"longitude": 10.5, "latitude": 20.3}

// New format (GeoJSON)
{"type": "Point", "coordinates": [10.5, 20.3]}
```
