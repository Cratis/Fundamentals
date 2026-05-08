// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection;

/// <summary>
/// Represents a convention-based service binding.
/// </summary>
/// <param name="ServiceType">Service type.</param>
/// <param name="ImplementationType">Implementation type.</param>
/// <param name="Lifetime">Service lifetime.</param>
public record ConventionServiceBinding(Type ServiceType, Type ImplementationType, ServiceLifetime Lifetime);
