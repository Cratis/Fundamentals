// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_EnumConverter.given;

#pragma warning disable CA1028, CA2217 // The non-int underlying type and the sign bit are the point of these specs.
[Flags]
public enum SByteFlags : sbyte
{
    None = 0,
    One = 1,
    Sign = sbyte.MinValue
}
