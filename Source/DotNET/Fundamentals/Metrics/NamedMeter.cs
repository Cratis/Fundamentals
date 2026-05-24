// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Metrics;

/// <summary>
/// Represents a typed named <see cref="Meter"/>.
/// </summary>
/// <typeparam name="T">Type the meter is for.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="NamedMeter{T}"/> class.
/// </remarks>
/// <param name="meter">The named meter resolved by inheriting the parent keyed service key.</param>
public class NamedMeter<T>([FromKeyedServices] Meter meter) : IMeter<T>
{
    /// <inheritdoc/>
    public Meter ActualMeter { get; } = meter;
}
