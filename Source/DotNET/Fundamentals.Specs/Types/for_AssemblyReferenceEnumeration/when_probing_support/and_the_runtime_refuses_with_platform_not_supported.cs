// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_AssemblyReferenceEnumeration.when_probing_support;

public class and_the_runtime_refuses_with_platform_not_supported : Specification
{
    bool _result;
    Exception _exception;

    void Because() => _exception = Catch.Exception(() =>
        _result = AssemblyReferenceEnumeration.Probe(() => throw new PlatformNotSupportedException()));

    [Fact] void should_report_unsupported() => _result.ShouldBeFalse();
    [Fact] void should_not_throw() => _exception.ShouldBeNull();
}
