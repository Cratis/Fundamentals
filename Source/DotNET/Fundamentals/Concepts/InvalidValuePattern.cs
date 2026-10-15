// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a value pattern is invalid or exceeds a construction budget.
/// </summary>
/// <param name="pattern">The invalid pattern.</param>
/// <param name="reason">The reason it cannot be used.</param>
public class InvalidValuePattern(string? pattern, string reason) : Exception($"Invalid value pattern '{pattern}': {reason}");
