// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.Metrics;

/// <summary>
/// Represents an unkeyed typed <see cref="Meter"/>.
/// </summary>
/// <typeparam name="T">Type the meter is for.</typeparam>
public class UnkeyedMeter<T> : IMeter<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnkeyedMeter{T}"/> class.
    /// </summary>
    public UnkeyedMeter()
    {
        ActualMeter = new(typeof(T).FullName ?? typeof(T).Name);
    }

    /// <inheritdoc/>
    public Meter ActualMeter { get; }
}