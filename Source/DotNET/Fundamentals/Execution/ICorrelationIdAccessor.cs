// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Execution;

/// <summary>
/// Defines a system that can access the current <see cref="CorrelationId"/>.
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Gets the current <see cref="CorrelationId"/>.
    /// </summary>
    CorrelationId Current { get; }
}