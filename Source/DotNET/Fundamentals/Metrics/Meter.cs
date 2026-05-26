// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Metrics;

/// <summary>
/// Represents a typed <see cref="Meter"/>.
/// </summary>
/// <typeparam name="T">Type the meter is for.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Meter{T}"/> class.
/// </remarks>
/// <param name="meter">The actual meter being used.</param>
/// <param name="serviceProvider">The service provider.</param>
/// <param name="serviceKey">The current service key, if any.</param>
public class Meter<T>(Meter? meter = null, IServiceProvider? serviceProvider = null, [ServiceKey] object? serviceKey = null) : IMeter<T>
{
    /// <inheritdoc/>
    public Meter ActualMeter { get; } =
        meter ??
        (serviceKey is not null && serviceProvider is not null ? serviceProvider.GetKeyedService<Meter>(serviceKey) : null) ??
        new(typeof(T).FullName ?? typeof(T).Name);
}
