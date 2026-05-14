// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Cratis.Reflection;
using Cratis.Strings;

namespace Cratis.Json;

/// <summary>
/// Represents a <see cref="JsonConverter{T}"/> that can convert types with properties that are of type object.
/// </summary>
/// <typeparam name="TTarget">Type of target the converter is for.</typeparam>
public abstract class TypeWithObjectPropertiesJsonConverter<TTarget> : JsonConverter<TTarget>
    where TTarget : class
{
    const string TypeProperty = "_type";

    /// <summary>
    /// Get the properties that are of type object.
    /// </summary>
    protected abstract IEnumerable<string> ObjectProperties { get; }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Type resolution and JSON deserialization use types stored in the JSON payload that are preserved at startup.")]
    [UnconditionalSuppressMessage("Trimming", "IL2057", Justification = "The type string in the JSON payload is expected to resolve to a type preserved by the application.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Properties set after deserialization are well-known named properties preserved by callers.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Type resolution and JSON deserialization use types stored in the JSON payload that are safe for AOT.")]
    public override TTarget? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonElement.ParseValue(ref reader);
        if (node.ValueKind == JsonValueKind.Object)
        {
            var nodeAsObject = JsonObject.Create(node);
            if (nodeAsObject?.ContainsKey(TypeProperty) == true)
            {
                var typeString = nodeAsObject[TypeProperty];
                var type = Type.GetType(typeString?.ToString() ?? string.Empty);
                if (type is not null)
                {
                    var valuesPerProperty = new Dictionary<string, object?>();

                    foreach (var property in ObjectProperties)
                    {
                        var jsonProperty = property.ToCamelCase();
                        var value = nodeAsObject[jsonProperty];
                        var valueTypeString = nodeAsObject[$"_{jsonProperty}Type"]?.ToString();
                        if (value is not null && valueTypeString is not null)
                        {
                            var valueType = Type.GetType(valueTypeString);
                            if (valueType is not null)
                            {
                                valuesPerProperty[property] = value.Deserialize(valueType, options);
                            }
                        }

                        nodeAsObject.Remove(jsonProperty);
                    }

                    var result = node.Deserialize(type, Globals.JsonSerializerOptions);
                    if (result is not null)
                    {
                        foreach (var property in ObjectProperties.Where(valuesPerProperty.ContainsKey))
                        {
                            var propertyValue = valuesPerProperty[property];
                            if (propertyValue is not null)
                            {
                                typeToConvert.GetProperty(property)?.SetValue(result, propertyValue);
                            }
                        }

                        return result as TTarget;
                    }
                }
            }
        }

        return default;
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization uses types stored in the JSON payload that are preserved at startup.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The runtime type's properties are accessed via well-known property names; the type is preserved by callers.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization uses types stored in the JSON payload that are safe for AOT.")]
    public override void Write(Utf8JsonWriter writer, TTarget value, JsonSerializerOptions options)
    {
        var type = value?.GetType();
        if (type is null)
        {
            return;
        }

        var node = JsonSerializer.SerializeToElement(value, type, Globals.JsonSerializerOptions);
        if (node.ValueKind == JsonValueKind.Object)
        {
            var nodeAsObject = JsonObject.Create(node);
            if (nodeAsObject is not null)
            {
                nodeAsObject["_type"] = type.GetTypeString();

                foreach (var property in ObjectProperties)
                {
                    var jsonProperty = property.ToCamelCase();
                    if (nodeAsObject.ContainsKey(jsonProperty))
                    {
                        var propertyValue = type.GetProperty(property)?.GetValue(value);
                        if (propertyValue is not null)
                        {
                            var propertyValueType = propertyValue.GetType();
                            nodeAsObject[$"_{jsonProperty}Type"] = propertyValueType.GetTypeString();
                            nodeAsObject[jsonProperty] = JsonSerializer.SerializeToNode(propertyValue, propertyValueType, options);
                        }
                    }
                }

                nodeAsObject.WriteTo(writer);
            }
        }
    }
}
