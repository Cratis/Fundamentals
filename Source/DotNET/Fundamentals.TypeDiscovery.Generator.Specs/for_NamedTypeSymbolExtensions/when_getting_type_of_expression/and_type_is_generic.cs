// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_NamedTypeSymbolExtensions.when_getting_type_of_expression;

public class and_type_is_generic : Specification
{
    INamedTypeSymbol _type;
    string _result;

    void Establish() =>
        _type = CompilationFactory.GetNamedTypes("namespace Foo { public class Box<T> { } }").Single(t => t.Name == "Box");

    void Because() => _result = _type.GetTypeOfExpression();

    [Fact] void should_return_open_generic_expression() => _result.ShouldEqual("global::Foo.Box<>");
}
