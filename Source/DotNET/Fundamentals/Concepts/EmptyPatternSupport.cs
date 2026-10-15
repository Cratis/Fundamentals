// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when no pattern values have an emittable check digit.
/// </summary>
/// <param name="pattern">The pattern with no admissible values.</param>
public class EmptyPatternSupport(string pattern) : Exception($"Value pattern '{pattern}' has no values with an emittable check digit.");
