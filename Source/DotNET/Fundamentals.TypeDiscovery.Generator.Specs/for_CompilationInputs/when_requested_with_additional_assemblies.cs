// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_CompilationInputs;

public class when_requested_with_additional_assemblies : Specification
{
    CSharpCompilation _first;
    CSharpCompilation _second;
    CSharpCompilation _default;

    void Because()
    {
        _first = CompilationFactory.CreateCompilation("public class First { }", typeof(FactAttribute).Assembly);
        _second = CompilationFactory.CreateCompilation("public class Second { }", typeof(FactAttribute).Assembly);
        _default = CompilationFactory.CreateCompilation("public class Default { }");
    }

    [Fact] void should_include_the_additional_reference() => _first.References.OfType<PortableExecutableReference>().Any(reference => reference.FilePath == typeof(FactAttribute).Assembly.Location).ShouldBeTrue();
    [Fact] void should_reuse_the_reference_for_the_same_assembly() => _first.References.Last().ShouldBeSame(_second.References.Last());
    [Fact] void should_share_the_default_references() => _default.References.Zip(_first.References).All(pair => ReferenceEquals(pair.First, pair.Second)).ShouldBeTrue();
    [Fact] void should_not_add_the_additional_reference_to_default_compilations() => _default.References.OfType<PortableExecutableReference>().Any(reference => reference.FilePath == typeof(FactAttribute).Assembly.Location).ShouldBeFalse();
}
