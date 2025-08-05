// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Humanizer;

namespace Cratis.Serialization;

/// <summary>
/// Default implementation of <see cref="INamingPolicy"/>.
/// </summary>
/// <param name="pluralizeReadModelNames">Whether to pluralize the read model names. Defaults to true.</param>
public class DefaultNamingPolicy(bool pluralizeReadModelNames = true) : NamingPolicy
{
    /// <inheritdoc/>
    public override JsonNamingPolicy? JsonPropertyNamingPolicy => null!;

    /// <inheritdoc/>
    protected override string GetReadModelNameImplementation(Type readModelType) => pluralizeReadModelNames ? readModelType.Name.Pluralize() : readModelType.Name;

    /// <inheritdoc/>
    protected override string GetPropertyNameImplementation(string name) => name;
}
