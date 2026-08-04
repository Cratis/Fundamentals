// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_Types;

public class when_generated_providers_are_registered : Specification
{
    Types _types;

    void Because()
    {
        GeneratedTypeDiscoveryRegistry.Register(new for_GeneratedTypeDiscoveryRegistry.a_provider());
        _types = new Types();
    }

    [Fact] void should_report_generated_discovery() => _types.DiscoveryMode.ShouldEqual(TypeDiscoveryMode.Generated);
    [Fact] void should_name_the_assembly_that_contributed() => _types.Assemblies.ShouldContain(typeof(for_GeneratedTypeDiscoveryRegistry.a_provider).Assembly);
}
