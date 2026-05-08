// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoveryCollector.when_getting_convention_service_bindings;

public class and_one_concrete_class_implements_matching_interface : Specification
{
    INamedTypeSymbol[] _symbols;
    (string ServiceExpression, string ImplementationExpression, string LifetimeExpression)[] _result;

    void Establish() =>
        _symbols =
        [
            .. CompilationFactory.GetNamedTypes(
                "namespace App;\n" +
                "public interface IFoo { }\n" +
                "public class Foo : IFoo { }")
        ];

    void Because() => _result = [.. TypeDiscoveryCollector.GetConventionServiceBindings(_symbols)];

    [Fact] void should_have_one_binding() => _result.Length.ShouldEqual(1);
    [Fact] void should_map_the_interface_as_service() => _result[0].ServiceExpression.ShouldEqual("global::App.IFoo");
    [Fact] void should_map_the_class_as_implementation() => _result[0].ImplementationExpression.ShouldEqual("global::App.Foo");
    [Fact] void should_use_transient_lifetime() => _result[0].LifetimeExpression.ShouldEqual("global::Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient");
}
