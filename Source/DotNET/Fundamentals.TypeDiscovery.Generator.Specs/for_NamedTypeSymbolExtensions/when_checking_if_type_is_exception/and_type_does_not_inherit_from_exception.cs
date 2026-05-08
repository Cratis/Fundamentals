// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_NamedTypeSymbolExtensions.when_checking_if_type_is_exception;

public class and_type_does_not_inherit_from_exception : Specification
{
    INamedTypeSymbol _type;
    bool _result;

    void Establish() =>
        _type = CompilationFactory.GetNamedTypes("public class MyService { }")
            .Single(t => t.Name == "MyService");

    void Because() => _result = _type.IsExceptionType();

    [Fact] void should_return_false() => _result.ShouldBeFalse();
}
