// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_NamedTypeSymbolExtensions.when_checking_if_in_system_or_microsoft_namespace;

public class and_type_is_in_system_namespace : Specification
{
    INamedTypeSymbol _type;
    bool _result;

    void Establish() =>
        _type = CompilationFactory.GetNamedTypes("namespace System.Foo { public class Thing { } }")
            .Single(t => t.Name == "Thing");

    void Because() => _result = _type.IsInSystemOrMicrosoftNamespace();

    [Fact] void should_return_true() => _result.ShouldBeTrue();
}
