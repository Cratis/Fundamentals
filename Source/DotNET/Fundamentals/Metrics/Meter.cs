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
/// <param name="meter">The actual meter being used. If resolved as a keyed service, the keyed <see cref="Meter"/> is used. If no keyed meter is available, a meter named from <typeparamref name="T"/> is created.</param>
public class Meter<T>([FromKeyedServices] Meter? meter = null) : IMeter<T>
{
    /// <inheritdoc/>
    public Meter ActualMeter { get; } = meter ?? new(typeof(T).FullName ?? typeof(T).Name);
}
