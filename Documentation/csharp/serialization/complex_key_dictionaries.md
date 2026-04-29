# Complex Key Dictionaries

The default JSON object model uses string property names. This means dictionaries with complex object keys need a custom conversion strategy to preserve typed keys across serialization boundaries.

## Overview

Fundamentals configures a `ComplexKeyDictionaryJsonConverterFactory` in global JSON options. The converter applies automatically to `IDictionary<TKey, TValue>` when `TKey` is a complex type.

For each dictionary entry:

1. The key object is serialized to compact JSON and used as the property name.
2. The value is serialized using the configured `JsonSerializerOptions`.
3. During deserialization, the key JSON string is deserialized back to the original key type.

## Registration

The converter is included in `Cratis.Json.Globals.Configure(...)` and therefore available anywhere Fundamentals JSON defaults are used.

## Example

```csharp
public record AccountKey(Guid Id, string Tenant);
public record AccountSummary(int Balance);

var options = Cratis.Json.Globals.JsonSerializerOptions;

IDictionary<AccountKey, AccountSummary> source =
    new Dictionary<AccountKey, AccountSummary>
    {
        [new AccountKey(Guid.Parse("e094c582-9d91-4df0-b795-8d39fce089da"), "tenant-a")] = new AccountSummary(42)
    };

var json = JsonSerializer.Serialize(source, options);
var roundTrip = JsonSerializer.Deserialize<IDictionary<AccountKey, AccountSummary>>(json, options);
```

## Frontend Interop

For frontend usage and typed key lookup, see [ValueMap](../../typescript/serialization/value_map.md).
