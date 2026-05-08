// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.AOT.Specs;

internal sealed class GenericDemo : IGenericDemo<int>
{
    public string GetValueTypeName() => nameof(Int32);
}
