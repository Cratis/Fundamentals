// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TypeDiscovery;

[Cratis.DependencyInjection.Singleton]
internal sealed class OnlySelfBound
{
    public override string ToString() => nameof(OnlySelfBound);
}
