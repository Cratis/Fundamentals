---
description: Serialize and bind DateOnly and TimeOnly in .NET with the System.Text.Json converters and type converters in Cratis.Fundamentals.
---
# Date & Time Only

In .NET 6, two new types were introduced: `DateOnly` and `TimeOnly`.
These are unfortunately not fully supported in serializers and typically require type conversion for use with
Web APIs. Fundamentals offers serializers and type converters for these types, which are automatically hooked up
in the application model.

## System.Text.Json converters

Within the **Fundamentals** package you'll find a namespace called `Json`. This holds converters for serializing and deserializing both `DateOnly` and `TimeOnly` types.

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new DateOnlyJsonConverter(),
        new TimeOnlyJsonConverter()
    }
};

var account = new Account
{
    RegisteredDate = new DateOnly(2022, 8, 19),
    RegisteredTime = new TimeOnly(13, 37, 0)
};
var serialized = JsonSerializer.Serialize(account, options);
```

The output would be:

```json
{"RegisteredDate":"2022-08-19","RegisteredTime":"13:37:00.0000000"}
```

These options only add the two converters, so property names keep their C# casing (`RegisteredDate`). Camel-case naming is a separate setting, for example the global options in `Cratis.Json.Globals`, which also register these converters.

The same two converters deserialize the values correctly. Use options with both converters, and match the property names to those options:

```csharp
using System.Text.Json;
using Cratis.Json;

var options = new JsonSerializerOptions
{
    Converters =
    {
        new DateOnlyJsonConverter(),
        new TimeOnlyJsonConverter()
    }
};

var json = "{ \"RegisteredDate\": \"2022-08-19\", \"RegisteredTime\": \"13:37:00.0000000\" }";
var account = JsonSerializer.Deserialize<Account>(json, options);
```

> Note: If you're using the Cratis Application Model, you do not have to configure this. It is automatically configured for the ASP.NET pipelines
> and other parts that needs it, such as the Cratis Kernel transports.

## Type Converters


The .NET component model has the concept of a [type converter](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.typeconverter?view=net-6.0).
These are leveraged by different parts of the .NET platform, including ASP.NET when dealing with things like parameters for controllers.
Out of the box, **Fundamentals** provides a type converter for the `DateOnly` and `TimeOnly` types. All you need to do is register them.

```csharp
using System.ComponentModel;
using Cratis.Conversion;

TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));
TypeDescriptor.AddAttributes(typeof(TimeOnly), new TypeConverterAttribute(typeof(TimeOnlyTypeConverter)));
```

There is a convenience method for registering these two type converters in **Fundamentals**:

```csharp
using Cratis.Conversion;

TypeConverters.Register();
```

> Note: If you're using the Cratis Application Model, you do not have to manually set this up. It is automatically configured at startup.
