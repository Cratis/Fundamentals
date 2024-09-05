// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;

namespace Cratis.Execution;

/// <summary>
/// Represents an implementation of <see cref="ICorrelationIdAccessor"/>.
/// </summary>
[Singleton]
public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    static readonly AsyncLocal<CorrelationId> _current = new();

    /// <inheritdoc/>
    public CorrelationId Current => _current.Value ?? Guid.Empty;

    /// <summary>
    /// Internal: Set the current correlation ID.
    /// </summary>
    /// <param name="correlationId"><see cref="CorrelationId"/> to set.</param>
    public static void SetCurrent(CorrelationId correlationId) => _current.Value = correlationId;
}
