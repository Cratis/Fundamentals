// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoveryCollector.when_getting_convention_self_bindings;

public class and_type_is_abstract : Specification
{
    INamedTypeSymbol[] _symbols;
    (string ImplementationExpression, string LifetimeExpression)[] _result;

    void Establish() =>
        _symbols = [.. CompilationFactory.GetNamedTypes("namespace App { public abstract class Base { } }")];

    void Because() => _result = [.. TypeDiscoveryCollector.GetConventionSelfBindings(_symbols)];

    [Fact] void should_not_include_the_abstract_type() => _result.Select(e => e.ImplementationExpression).ShouldNotContain("global::App.Base");
}
