// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_NamedTypeSymbolExtensions.when_getting_type_of_expression;

public class and_type_is_not_generic : Specification
{
    INamedTypeSymbol _type;
    string _result;

    void Establish() =>
        _type = CompilationFactory.GetNamedTypes("namespace Foo { public class MyClass { } }").Single(t => t.Name == "MyClass");

    void Because() => _result = _type.GetTypeOfExpression();

    [Fact] void should_return_fully_qualified_name() => _result.ShouldEqual("global::Foo.MyClass");
}
