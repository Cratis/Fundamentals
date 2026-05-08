// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types;

/// <summary>
/// Defines a system that can provide precomputed contract to implementor mappings for type discovery.
/// </summary>
public interface ICanProvideContractToImplementorsForDiscovery
{
    /// <summary>
    /// Gets the precomputed contracts and implementors.
    /// </summary>
    IDictionary<Type, IEnumerable<Type>> ContractsAndImplementors { get; }
}