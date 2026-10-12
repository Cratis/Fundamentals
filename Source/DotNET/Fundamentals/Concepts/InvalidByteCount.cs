// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a random byte count is outside the supported range.
/// </summary>
/// <param name="count">The invalid byte count.</param>
public class InvalidByteCount(int count) : Exception($"Random byte count '{count}' must be from 1 to 512.");
