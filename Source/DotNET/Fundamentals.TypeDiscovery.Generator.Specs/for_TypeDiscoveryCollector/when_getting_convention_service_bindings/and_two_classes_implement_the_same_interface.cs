// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoveryCollector.when_getting_convention_service_bindings;

public class and_two_classes_implement_the_same_interface : Specification
{
    INamedTypeSymbol[] _symbols;
    (string ServiceExpression, string ImplementationExpression, string LifetimeExpression)[] _result;

    void Establish() =>
        _symbols =
        [
            .. CompilationFactory.GetNamedTypes(
                "namespace App;\n" +
                "public interface IFoo { }\n" +
                "public class Foo : IFoo { }\n" +
                "public class AnotherFoo : IFoo { }")
        ];

    void Because() => _result = [.. TypeDiscoveryCollector.GetConventionServiceBindings(_symbols)];

    [Fact] void should_produce_no_bindings() => _result.ShouldBeEmpty();
}
