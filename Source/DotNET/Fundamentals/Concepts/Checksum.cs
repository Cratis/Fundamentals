// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Identifies the check digits appended to a generated pattern.
/// </summary>
public enum Checksum
{
    /// <summary>
    /// No check digits.
    /// </summary>
    None = 0,

    /// <summary>
    /// One Luhn check digit.
    /// </summary>
    Luhn = 1,

    /// <summary>
    /// One modulus-11 check digit, excluding payloads whose check would be 10.
    /// </summary>
    Mod11 = 2,

    /// <summary>
    /// Two ISO 7064 MOD 97-10 check digits.
    /// </summary>
    Mod97 = 3
}
