// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TypeDiscovery;

[Cratis.DependencyInjection.Scoped]
internal sealed class ScopedSelfBinding
{
    public string Describe() => "Registered through AddSelfBindings";
}
