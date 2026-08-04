// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_Types;

public class when_providers_are_supplied_by_the_caller : Specification
{
    Types _types;

    void Because() => _types = new Types([new plain_assembly_provider()]);

    [Fact] void should_report_explicit_discovery() => _types.DiscoveryMode.ShouldEqual(TypeDiscoveryMode.Explicit);
    [Fact] void should_name_the_assembly_that_contributed() => _types.Assemblies.ShouldContain(typeof(plain_assembly_provider).Assembly);
}
