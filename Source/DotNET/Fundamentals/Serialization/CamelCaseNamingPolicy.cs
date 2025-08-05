// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Strings;
using Humanizer;

namespace Cratis.Serialization;

/// <summary>
/// Camel case implementation of <see cref="INamingPolicy"/>.
/// </summary>
/// <param name="pluralizeReadModelNames">Whether to pluralize the read model names. Defaults to true.</param>
public class CamelCaseNamingPolicy(bool pluralizeReadModelNames = true) : NamingPolicy
{
    /// <inheritdoc/>
    public override JsonNamingPolicy JsonPropertyNamingPolicy => AcronymFriendlyJsonCamelCaseNamingPolicy.Instance;

    /// <inheritdoc/>
    protected override string GetReadModelNameImplementation(Type readModelType) => pluralizeReadModelNames
        ? readModelType.Name.Pluralize().ToCamelCase()
        : readModelType.Name.ToCamelCase();

    /// <inheritdoc/>
    protected override string GetPropertyNameImplementation(string name) => name.ToCamelCase();
}
