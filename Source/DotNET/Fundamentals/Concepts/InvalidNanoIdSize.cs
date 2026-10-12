// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a NanoID size is outside the supported range.
/// </summary>
/// <param name="size">The invalid character count.</param>
public class InvalidNanoIdSize(int size) : Exception($"NanoID size '{size}' must be from 1 to 256 characters.");
