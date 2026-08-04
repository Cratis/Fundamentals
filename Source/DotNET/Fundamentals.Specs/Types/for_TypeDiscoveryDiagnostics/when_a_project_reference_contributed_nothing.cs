// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeDiscoveryDiagnostics;

public class when_a_project_reference_contributed_nothing : Specification
{
    ITypes _types;
    string[] _missing;

    void Establish() => _types = new Types([new for_Types.plain_assembly_provider()]);

    void Because() => _missing = [.. TypeDiscoveryDiagnostics.FindMissingContributors(_types)];

    [Fact] void should_name_the_package_the_universe_never_reached() => _missing.ShouldContain(typeof(Types).Assembly.GetName().Name);
    [Fact] void should_not_name_the_assembly_that_did_contribute() => _missing.ShouldNotContain(typeof(when_a_project_reference_contributed_nothing).Assembly.GetName().Name);
}
