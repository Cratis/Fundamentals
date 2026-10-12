// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Carries the operation's captured time and portable tenant identifier for value generation.
/// </summary>
/// <param name="Now">The instant the operation was received, captured once per command so every value in it agrees.</param>
/// <param name="Tenant">The portable tenant identifier: <see cref="DefaultTenant"/> for the default tenant, or a named tenant's identifier.</param>
public readonly record struct GenerationContext(DateTimeOffset Now, string Tenant)
{
    /// <summary>
    /// The zero-GUID identifier of the default tenant.
    /// </summary>
    public const string DefaultTenant = "00000000-0000-0000-0000-000000000000";
}
