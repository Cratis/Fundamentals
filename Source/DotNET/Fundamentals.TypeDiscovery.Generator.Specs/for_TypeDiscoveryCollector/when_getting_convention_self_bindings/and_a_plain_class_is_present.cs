// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoveryCollector.when_getting_convention_self_bindings;

public class and_a_plain_class_is_present : Specification
{
    INamedTypeSymbol[] _symbols;
    (string ImplementationExpression, string LifetimeExpression)[] _result;

    void Establish() =>
        _symbols = CompilationFactory.GetNamedTypes("namespace App { public class MyService { } }")
            .ToArray();

    void Because() => _result = TypeDiscoveryCollector.GetConventionSelfBindings(_symbols).ToArray();

    [Fact] void should_include_the_class() => _result.Select(e => e.ImplementationExpression).ShouldContain("global::App.MyService");
}
