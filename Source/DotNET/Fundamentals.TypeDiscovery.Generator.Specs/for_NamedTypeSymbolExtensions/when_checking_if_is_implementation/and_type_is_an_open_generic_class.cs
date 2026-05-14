// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_NamedTypeSymbolExtensions.when_checking_if_is_implementation;

public class and_type_is_an_open_generic_class : Specification
{
    INamedTypeSymbol _type;
    bool _result;

    void Establish() =>
        _type = CompilationFactory.GetNamedTypes("public class Generic<T> { }").Single(t => t.Name == "Generic");

    void Because() => _result = _type.IsImplementation();

    [Fact] void should_return_false() => _result.ShouldBeFalse();
}
