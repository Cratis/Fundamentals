// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json;

[Flags]
public enum MyFlags
{
    None = 0,
    First = 1,
    Second = 1 << 1,
    Third = 1 << 2
}
