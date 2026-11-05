---
title: Complex key dictionaries in Cratis Fundamentals
description: Cratis Fundamentals serializes .NET dictionaries with complex object keys as JSON property names and reconstructs typed keys when reading.
---
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
using System.Text.Json;

var options = Cratis.Json.Globals.JsonSerializerOptions;

IDictionary<AccountKey, AccountSummary> source =
    new Dictionary<AccountKey, AccountSummary>
    {
        [new AccountKey(Guid.Parse("e094c582-9d91-4df0-b795-8d39fce089da"), "tenant-a")] = new AccountSummary(42)
    };

var json = JsonSerializer.Serialize(source, options);
var roundTrip = JsonSerializer.Deserialize<IDictionary<AccountKey, AccountSummary>>(json, options);

public record AccountKey(Guid Id, string Tenant);
public record AccountSummary(int Balance);
```

In the example, each AccountKey becomes compact JSON inside a JSON property name; it is not a nested key object alongside the value. Reading `roundTrip` reconstructs an AccountKey from that property name and an AccountSummary from its value. Keep the same key-conversion settings on both sides: the converter copies the supplied options for keys and turns off indentation. If the payload instead uses a plain identifier as the property name, compare it with this key format before treating it as an AccountKey dictionary.

## Frontend Interop

For frontend usage and typed key lookup, see [ValueMap](../../typescript/serialization/value_map.md).
