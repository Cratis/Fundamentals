---
title: DateOnly and TimeOnly in Cratis Fundamentals
description: Serialize and bind DateOnly and TimeOnly in .NET with the System.Text.Json converters and type converters in Cratis.Fundamentals.
---
# Date & Time Only

In .NET 6, two new types were introduced: `DateOnly` and `TimeOnly`.
Choose the conversion boundary first. For a JSON body, use the JSON options and converters below. For a controller parameter or another component-model conversion, use the type converters in the second section; registering them does not configure JSON serialization.

## System.Text.Json converters

Within the **Fundamentals** package you'll find a namespace called `Json`. This holds converters for serializing and deserializing both `DateOnly` and `TimeOnly` types.

The following serialization and deserialization examples are separate fragments. Both assume an application-defined Account with RegisteredDate and RegisteredTime properties of those types. Use the first fragment to compare the written values and property names, then the second to read that shape back; do not paste both fragments into one scope.

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

> Note: Cratis Arc includes these converters in its default JSON options and copies them to MVC controller JSON options when controller support is enabled. Cratis Chronicle registers them for the Kernel's Microsoft Orleans JSON serialization. For separately configured `JsonSerializerOptions`, add the converters as shown above.

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

> Note: Cratis Arc calls `TypeConverters.Register()` when its core services are added, so you do not need to repeat that registration.
