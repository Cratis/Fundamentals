// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when a checksum algorithm is undefined or none is supplied for validation.
/// </summary>
/// <param name="checksum">The unsupported algorithm.</param>
public class UnknownChecksum(Checksum checksum) : Exception($"Checksum '{checksum}' is not a supported checksum algorithm for this operation.");
