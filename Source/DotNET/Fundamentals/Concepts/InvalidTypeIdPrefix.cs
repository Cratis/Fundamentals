// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a TypeID prefix does not satisfy its format.
/// </summary>
/// <param name="prefix">The invalid prefix.</param>
public class InvalidTypeIdPrefix(string? prefix) : Exception($"TypeID prefix '{prefix ?? "<null>"}' must be empty or contain at most 63 lowercase letters or underscores, starting and ending with a letter.");
