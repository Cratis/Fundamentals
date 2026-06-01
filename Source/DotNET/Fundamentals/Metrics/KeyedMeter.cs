// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Metrics;

/// <summary>
/// Represents a keyed typed <see cref="Meter"/>.
/// </summary>
/// <typeparam name="T">Type the meter is for.</typeparam>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/> used for resolving keyed services.</param>
/// <param name="key">The key used when resolving the service.</param>
public class KeyedMeter<T>(IServiceProvider serviceProvider, [ServiceKey] string? key = null) : IMeter<T>
{
    /// <inheritdoc/>
    public Meter ActualMeter { get; } = key is null
        ? new(typeof(T).FullName ?? typeof(T).Name)
        : serviceProvider.GetRequiredKeyedService<Meter>(key);
}