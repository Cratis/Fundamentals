// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.for_CompilationInputs;

public class when_requested_concurrently : Specification
{
    CSharpCompilation[] _compilations;

    async Task Because() => _compilations = await Task.WhenAll(Enumerable.Range(0, 16)
        .Select(i => Task.Run(() => CompilationFactory.CreateCompilation($"public class C{i} {{ }}"))));

    [Fact] void should_share_the_same_metadata_reference_instances() => _compilations.All(compilation => compilation.References.Zip(_compilations[0].References).All(pair => ReferenceEquals(pair.First, pair.Second))).ShouldBeTrue();
    [Fact] void should_share_the_same_compilation_options() => _compilations.All(compilation => ReferenceEquals(compilation.Options, _compilations[0].Options)).ShouldBeTrue();
    [Fact] void should_share_the_same_parse_options() => _compilations.All(compilation => ReferenceEquals(compilation.SyntaxTrees.Single().Options, _compilations[0].SyntaxTrees.Single().Options)).ShouldBeTrue();
    [Fact] void should_create_a_separate_compilation_for_each_request() => _compilations.Distinct().Count().ShouldEqual(16);
    [Fact] void should_start_the_inputs_once() => CompilationInputs.Starts.ShouldEqual(1);
}
