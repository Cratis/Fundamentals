// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;

namespace Cratis.Configuration;

/// <summary>
/// Represents a resolver that is able to resolve the instance of a property on a configuration object.
/// </summary>
public interface IConfigurationValueResolver
{
    /// <summary>
    /// Resolve the value based on the <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> for the root object.</param>
    /// <returns>Resolved value.</returns>
    object Resolve(IConfiguration configuration);
}
