// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection;

/// <summary>
/// Represents a convention-based self binding.
/// </summary>
/// <param name="ImplementationType">Implementation type.</param>
/// <param name="Lifetime">Service lifetime.</param>
public record ConventionSelfBinding(Type ImplementationType, ServiceLifetime Lifetime);
