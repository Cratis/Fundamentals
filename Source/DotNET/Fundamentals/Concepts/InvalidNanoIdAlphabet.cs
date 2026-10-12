// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a NanoID alphabet is not distinct printable ASCII of the supported length.
/// </summary>
public class InvalidNanoIdAlphabet() : Exception("A NanoID alphabet must contain 2 to 256 distinct printable ASCII characters (0x20 through 0x7E).");
