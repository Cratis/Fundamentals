// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reflection.for_PropertyExtensions;

public class ClassWithProperties
{
    public string NonNullableString { get; set; } = string.Empty;
    public string? NullableString { get; set; }
    public int NonNullableInt { get; set; }
    public int? NullableInt { get; set; }
}
