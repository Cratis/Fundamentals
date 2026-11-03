// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// The exception that is thrown when SHA-256 input contains an unpaired surrogate.
/// </summary>
public class InvalidSha256Input() : Exception("SHA-256 input must be well-formed UTF-16 text; it contains an unpaired surrogate.");
