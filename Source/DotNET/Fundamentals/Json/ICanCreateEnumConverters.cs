// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Cratis.Json;

/// <summary>
/// Defines enum converter creation implemented by generated discovery providers.
/// </summary>
public interface ICanCreateEnumConverters
{
    /// <summary>
    /// Creates a converter for an enum declared in the provider's assembly.
    /// </summary>
    /// <param name="enumType">The enum type to convert.</param>
    /// <returns>The converter, or null for an enum the assembly does not declare.</returns>
    JsonConverter? CreateEnumConverter(Type enumType);
}
