# Geospatial Coordinate

The `Coordinate` record represents a geographic location with longitude and latitude values. It provides JSON serialization support for both C# and TypeScript, making it easy to share geospatial data between frontend and backend.

## Basic Usage

Create a coordinate with longitude and latitude:

```csharp
using Cratis.Geospatial;

var location = new Coordinate(10.5, 20.3);
Console.WriteLine($"Longitude: {location.Longitude}"); // 10.5
Console.WriteLine($"Latitude: {location.Latitude}");   // 20.3
```

## JSON Serialization

The `CoordinateJsonConverter` is automatically registered in the global `JsonSerializerOptions` when using the Cratis Application Model.

### Serialization

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var coordinate = new Coordinate(10.5, 20.3);
var json = JsonSerializer.Serialize(coordinate, Globals.JsonSerializerOptions);
```

Output:

```json
{
    "longitude": 10.5,
    "latitude": 20.3
}
```

### Deserialization

```csharp
using System.Text.Json;
using Cratis.Geospatial;
using Cratis.Json;

var json = "{\"longitude\":10.5,\"latitude\":20.3}";
var coordinate = JsonSerializer.Deserialize<Coordinate>(json, Globals.JsonSerializerOptions);
```

### Validation

The converter validates that both `longitude` and `latitude` properties are present during deserialization. If either property is missing, a `JsonException` will be thrown.

## Manual Converter Configuration

If you need to manually configure the converter:

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new CoordinateJsonConverter()
    }
};
```

> **Note**: If you're using the Cratis Application Model, the converter is automatically configured for ASP.NET pipelines and other parts that need it, such as the Cratis Kernel transports.

## Integration with Models

Coordinate can be used as a property in your domain models:

```csharp
using Cratis.Geospatial;

public record Location(
    string Name,
    Coordinate Position
);

var office = new Location(
    "Headquarters",
    new Coordinate(10.7522, 59.9139) // Oslo, Norway
);
```
