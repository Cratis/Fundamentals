// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Cratis.Metrics;
using Cratis.Traces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DiagnosticsActivitySource = System.Diagnostics.ActivitySource;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/> for registering named diagnostics services.
/// </summary>
public static class DiagnosticsServiceCollectionExtensions
{
    /// <summary>
    /// Adds a named <see cref="Meter"/> and keyed typed <see cref="IMeter{T}"/> registrations.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <param name="name">Name of the meter.</param>
    /// <returns><see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddNamedMeter(this IServiceCollection services, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        services.TryAddKeyedSingleton(typeof(Meter), name, static (_, key) => new Meter((string)key!));
        services.TryAddKeyedSingleton(typeof(IMeter<>), name, typeof(KeyedMeter<>));

        return services;
    }

    /// <summary>
    /// Adds a named <see cref="DiagnosticsActivitySource"/> and keyed typed <see cref="IActivitySource{T}"/> registrations.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <param name="name">Name of the activity source.</param>
    /// <returns><see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddNamedActivitySource(this IServiceCollection services, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        services.TryAddKeyedSingleton(typeof(DiagnosticsActivitySource), name, static (_, key) => new DiagnosticsActivitySource((string)key!));
        services.TryAddKeyedSingleton(typeof(IActivitySource<>), name, typeof(KeyedActivitySource<>));

        return services;
    }
}
