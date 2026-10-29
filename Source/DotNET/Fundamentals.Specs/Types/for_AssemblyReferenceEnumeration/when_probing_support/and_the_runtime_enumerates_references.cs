// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_AssemblyReferenceEnumeration.when_probing_support;

public class and_the_runtime_enumerates_references : Specification
{
    bool _result;

    void Because() => _result = AssemblyReferenceEnumeration.Probe(() => []);

    [Fact] void should_report_supported() => _result.ShouldBeTrue();
}
