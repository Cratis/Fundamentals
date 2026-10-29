// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_AssemblyReferenceEnumeration.when_probing_support;

public class and_the_probe_fails_for_another_reason : Specification
{
    InvalidOperationException _failure;
    Exception _exception;

    void Establish() => _failure = new InvalidOperationException("Probe failed");

    void Because() => _exception = Catch.Exception(() =>
        AssemblyReferenceEnumeration.Probe(() => throw _failure));

    [Fact] void should_propagate_the_exception() => _exception.ShouldBeSame(_failure);
}
