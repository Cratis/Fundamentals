// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a pattern with date segments is generated without a context.
/// </summary>
/// <param name="pattern">The pattern requiring a captured time.</param>
public class PatternRequiresGenerationContext(string pattern) : Exception($"Value pattern '{pattern}' requires a generation context for its UTC date segments.");
