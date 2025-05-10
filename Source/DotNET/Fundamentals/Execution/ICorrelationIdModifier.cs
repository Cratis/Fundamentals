// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Execution;

/// <summary>
/// Defines a contract for modifying the current correlation ID.
/// </summary>
public interface ICorrelationIdModifier
{
    /// <summary>
    /// Modifies the current correlation ID.
    /// </summary>
    /// <param name="correlationId">The new correlation ID.</param>
    void Modify(CorrelationId correlationId);
}