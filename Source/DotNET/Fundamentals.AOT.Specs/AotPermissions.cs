// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.AOT.Specs;

[Flags]
internal enum AotPermissions
{
    /// <summary>No permissions.</summary>
    None = 0,

    /// <summary>Read permission.</summary>
    Read = 1,

    /// <summary>Write permission.</summary>
    Write = 2
}
