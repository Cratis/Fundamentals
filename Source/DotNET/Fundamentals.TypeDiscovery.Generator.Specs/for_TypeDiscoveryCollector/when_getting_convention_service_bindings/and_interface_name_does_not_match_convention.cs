// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoveryCollector.when_getting_convention_service_bindings;

public class and_interface_name_does_not_match_convention : Specification
{
    INamedTypeSymbol[] _symbols;
    (string ServiceExpression, string ImplementationExpression, string LifetimeExpression)[] _result;

    void Establish() =>
        _symbols = CompilationFactory.GetNamedTypes(
            """
            namespace App;
            public interface IBar { }
            public class Foo : IBar { }
            """)
            .ToArray();

    void Because() => _result = TypeDiscoveryCollector.GetConventionServiceBindings(_symbols).ToArray();

    [Fact] void should_produce_no_bindings() => _result.ShouldBeEmpty();
}
