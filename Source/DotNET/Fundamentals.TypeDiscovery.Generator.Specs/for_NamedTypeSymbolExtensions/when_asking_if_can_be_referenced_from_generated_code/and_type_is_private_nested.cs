// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_NamedTypeSymbolExtensions.when_asking_if_can_be_referenced_from_generated_code;

public class and_type_is_private_nested : Specification
{
    INamedTypeSymbol _type;
    bool _result;

    void Establish() =>
        _type = CompilationFactory.GetNamedTypes("public class Outer { private class Inner { } }")
            .Single(t => t.Name == "Inner");

    void Because() => _result = _type.CanBeReferencedFromGeneratedCode();

    [Fact] void should_return_false() => _result.ShouldBeFalse();
}
