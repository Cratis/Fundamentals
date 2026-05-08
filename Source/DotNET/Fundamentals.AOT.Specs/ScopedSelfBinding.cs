// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs;

[Scoped]
internal sealed class ScopedSelfBinding
{
    public string Describe() => "Registered through AddSelfBindings";
}
