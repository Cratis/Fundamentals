// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Cratis.Types;

namespace Cratis.Json;

/// <summary>
/// Finds enum converters created by generated discovery providers.
/// </summary>
internal static class GeneratedEnumConverters
{
    /// <summary>Tries to create a converter from registered providers.</summary>
    /// <param name="enumType">The enum type to convert.</param>
    /// <returns>A compatible converter, or null.</returns>
    internal static JsonConverter? TryCreate(Type enumType)
    {
        GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered();
        return TryCreate(enumType, GeneratedTypeDiscoveryRegistry.Providers);
    }

    /// <summary>Tries to create a converter from the supplied providers.</summary>
    /// <param name="enumType">The enum type to convert.</param>
    /// <param name="providers">The discovery providers to consult.</param>
    /// <returns>The first compatible converter, or null.</returns>
    internal static JsonConverter? TryCreate(Type enumType, IEnumerable<ICanProvideAssembliesForDiscovery> providers)
    {
        foreach (var provider in providers)
        {
            if (provider is ICanCreateEnumConverters factory &&
                factory.CreateEnumConverter(enumType) is { } converter &&
                converter.CanConvert(enumType))
            {
                return converter;
            }
        }

        return null;
    }
}
