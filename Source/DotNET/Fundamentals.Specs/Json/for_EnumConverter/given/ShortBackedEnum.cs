// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_EnumConverter.given;

#pragma warning disable CA1028 // The non-int underlying type is the point of these specs.
public enum ShortBackedEnum : short
{
    Min = short.MinValue,
    Max = short.MaxValue
}
