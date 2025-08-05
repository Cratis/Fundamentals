// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using Cratis.ReadModels;
using Cratis.Reflection;

namespace Cratis.Serialization;

/// <summary>
/// Represents a base class for naming policies.
/// </summary>
public abstract class NamingPolicy : INamingPolicy
{
    /// <inheritdoc/>
    public abstract JsonNamingPolicy? JsonPropertyNamingPolicy { get; }

    /// <inheritdoc/>
    public string GetReadModelName(Type readModelType)
    {
        if (readModelType.HasAttribute<ReadModelNameAttribute>())
        {
            return readModelType.GetCustomAttribute<ReadModelNameAttribute>(false)!.Name;
        }

        return GetReadModelNameImplementation(readModelType);
    }

    /// <inheritdoc/>
    public string GetPropertyName(string name) => GetPropertyNameImplementation(name);

    /// <summary>
    /// Gets the read model name for the specified type.
    /// </summary>
    /// <param name="readModelType">The type of the read model.</param>
    /// <returns>The desired name of the read model.</returns>
    protected abstract string GetReadModelNameImplementation(Type readModelType);

    /// <summary>
    /// Gets the property name from an existing name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The desired name of the property.</returns>
    protected abstract string GetPropertyNameImplementation(string name);
}