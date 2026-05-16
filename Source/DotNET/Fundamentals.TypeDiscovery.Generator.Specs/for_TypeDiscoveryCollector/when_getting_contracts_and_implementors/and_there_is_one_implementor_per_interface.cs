// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoveryCollector.when_getting_contracts_and_implementors;

public class and_there_is_one_implementor_per_interface : Specification
{
    INamedTypeSymbol[] _symbols;
    IAssemblySymbol _assembly;
    (string ContractExpression, System.Collections.Immutable.ImmutableArray<string> ImplementorExpressions)[] _result;

    void Establish()
    {
        var compilation = CompilationFactory.CreateCompilation(
            "namespace App;\n" +
            "public interface IFoo { }\n" +
            "public class Foo : IFoo { }");
        _assembly = compilation.Assembly;
        _symbols = [.. compilation.Assembly.GlobalNamespace.GetAllNamedTypes()];
    }

    void Because() => _result = [.. TypeDiscoveryCollector.GetContractsAndImplementors(_symbols, _assembly)];

    [Fact] void should_include_the_interface_as_a_contract() => _result.Select(e => e.ContractExpression).ShouldContain("global::App.IFoo");
    [Fact] void should_map_the_class_as_the_implementor() =>
        _result.Single(e => e.ContractExpression == "global::App.IFoo").ImplementorExpressions.ShouldContain("global::App.Foo");
}
