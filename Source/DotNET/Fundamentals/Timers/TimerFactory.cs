// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Timers;

/// <summary>
/// Represents an implementation of <see cref="ITimerFactory"/>.
/// </summary>
public class TimerFactory : ITimerFactory
{
    /// <inheritdoc/>
    public ITimer Create(TimerCallback callback, int dueTime, int period, object? state = null)
#pragma warning disable CA2000 // Dispose objects before losing scope
        => new Timer(new System.Threading.Timer(callback, state, dueTime, period));
#pragma warning restore CA2000 // Dispose objects before losing scope
}
