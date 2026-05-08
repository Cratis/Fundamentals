// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.DependencyInjection;

/// <summary>
/// Defines a system that can provide precomputed IoC convention metadata.
/// </summary>
public interface ICanProvideConventionsForDependencyInjection
{
    /// <summary>
    /// Gets precomputed service bindings for interface-to-implementation conventions.
    /// </summary>
    IEnumerable<ConventionServiceBinding> ConventionServiceBindings { get; }

    /// <summary>
    /// Gets precomputed self-bindings.
    /// </summary>
    IEnumerable<ConventionSelfBinding> SelfBindings { get; }
}