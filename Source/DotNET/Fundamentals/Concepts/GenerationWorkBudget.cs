// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Enforces the shared hard construction limits before performing counted work.
/// </summary>
/// <param name="pattern">The pattern named in budget diagnostics.</param>
/// <param name="limit">The maximum number of operations.</param>
internal sealed class GenerationWorkBudget(string pattern, long limit = GenerationWorkBudget.OperationLimit)
{
    /// <summary>
    /// The maximum number of unrolled character positions.
    /// </summary>
    internal const int PositionLimit = 2_048;

    /// <summary>
    /// The maximum number of distinct DFA or checksum-product states.
    /// </summary>
    internal const int StateLimit = 10_000;

    /// <summary>
    /// The maximum number of edge inspections and counting transitions.
    /// </summary>
    internal const long OperationLimit = 100_000_000;

    /// <summary>
    /// Gets the number of operations already performed.
    /// </summary>
    internal long Used { get; private set; }

    /// <summary>
    /// Charges operations before they are performed.
    /// </summary>
    /// <param name="operations">The nonnegative number of operations to charge.</param>
    /// <exception cref="InvalidValuePattern">The operation budget would be exceeded.</exception>
    internal void Spend(long operations = 1)
    {
        if (operations < 0 || operations > limit - Used)
        {
            Exceeded($"operation budget ({limit} operations)");
        }

        Used += operations;
    }

    /// <summary>
    /// Reports an exceeded hard construction budget.
    /// </summary>
    /// <param name="budget">The budget name and exact limit.</param>
    /// <exception cref="InvalidValuePattern">Always thrown with the pattern and budget reason.</exception>
    internal void Exceeded(string budget) => throw new InvalidValuePattern(pattern, $"Generation support {budget} exceeded.");
}
