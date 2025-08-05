// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Strings;
using Humanizer;

namespace Cratis.Serialization;

/// <summary>
/// Represents a naming policy that applies a namespaced naming convention.
/// </summary>
/// <param name="segmentsToSkip">Optionally number of segments in the namespace to skip. Defaults to 0.</param>
/// <param name="separator">Optional separator character to use between namespace segments. Defaults to '-'.</param>
/// <param name="prefix">Optional prefix to prepend all collection names with.</param>
/// <param name="camelCase">Whether to apply camel case to the names.</param>
/// <param name="pluralizeReadModelNames">Whether to pluralize the read model names. Defaults to true.</param>
public class NamespacedNamingPolicy(
    int segmentsToSkip = 0,
    char separator = '-',
    string prefix = "",
    bool camelCase = false,
    bool pluralizeReadModelNames = true) : NamingPolicy
{
    /// <inheritdoc/>
    public override JsonNamingPolicy? JsonPropertyNamingPolicy => camelCase ? AcronymFriendlyJsonCamelCaseNamingPolicy.Instance : null!;

    /// <inheritdoc/>
    protected override string GetReadModelNameImplementation(Type readModelType)
    {
        var segments = readModelType.Namespace?.Split('.') ?? [];
        var modelPrefix = string.Join(separator, segments.Skip(segmentsToSkip).Select(_ => _.ToCamelCase()));
        var name = pluralizeReadModelNames
            ? readModelType.Name.Pluralize()
            : readModelType.Name;
        name = camelCase ? name.ToCamelCase() : name;
        return $"{prefix}{modelPrefix}-{name}";
    }

    /// <inheritdoc/>
    protected override string GetPropertyNameImplementation(string name) => camelCase ? name.ToCamelCase() : name;
}
