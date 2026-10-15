// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when an integer generation range is inverted.
/// </summary>
/// <param name="minimum">The inclusive lower bound.</param>
/// <param name="maximum">The inclusive upper bound.</param>
public class InvalidIntegerRange(long minimum, long maximum) : Exception($"Integer generation minimum '{minimum}' exceeds maximum '{maximum}'.");
