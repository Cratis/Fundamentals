// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoveryCollector.when_getting_contracts_and_implementors;

public class and_there_are_multiple_implementors_for_one_interface : Specification
{
    INamedTypeSymbol[] _symbols;
    IAssemblySymbol _assembly;
    System.Collections.Immutable.ImmutableArray<string> _implementors;

    void Establish()
    {
        var compilation = CompilationFactory.CreateCompilation(
            "namespace App;\n" +
            "public interface IHandler { }\n" +
            "public class HandlerA : IHandler { }\n" +
            "public class HandlerB : IHandler { }");
        _assembly = compilation.Assembly;
        _symbols = [.. compilation.Assembly.GlobalNamespace.GetAllNamedTypes()];
    }

    void Because() =>
        _implementors = TypeDiscoveryCollector.GetContractsAndImplementors(_symbols, _assembly)
            .Single(e => e.ContractExpression == "global::App.IHandler")
            .ImplementorExpressions;

    [Fact] void should_include_first_implementor() => _implementors.ShouldContain("global::App.HandlerA");
    [Fact] void should_include_second_implementor() => _implementors.ShouldContain("global::App.HandlerB");
}
