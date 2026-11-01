// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_generating_enum_converters;

public class and_the_assembly_declares_no_enums : given.an_enum_compilation
{
    void Because() => Generate("namespace App; public class SomeClass { }");

    [Fact] void should_not_implement_the_converter_contract() => generated_source.ShouldNotContain("ICanCreateEnumConverters");
    [Fact] void should_not_emit_a_factory_method() => generated_source.ShouldNotContain("CreateEnumConverter");
    [Fact] void should_compile_without_errors() => errors.ShouldBeEmpty();
}
