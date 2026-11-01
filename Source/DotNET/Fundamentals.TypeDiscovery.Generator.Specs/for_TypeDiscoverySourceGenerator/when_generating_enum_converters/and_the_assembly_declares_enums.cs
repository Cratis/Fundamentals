// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_generating_enum_converters;

public class and_the_assembly_declares_enums : given.an_enum_compilation
{
    void Because() => Generate("""
        namespace App;
        public enum Color { Red }
        internal enum Hidden { None }
        public class Outer { public enum Kind { None } }
        public class SomeClass { }
        """);

    [Fact] void should_implement_the_converter_contract() => generated_source.ShouldContain("global::Cratis.Json.ICanCreateEnumConverters");
    [Fact] void should_create_a_public_enum_converter() => generated_source.ShouldContain("new global::Cratis.Json.EnumConverter<global::App.Color>()");
    [Fact] void should_create_an_internal_enum_converter() => generated_source.ShouldContain("new global::Cratis.Json.EnumConverter<global::App.Hidden>()");
    [Fact] void should_create_a_nested_enum_converter() => generated_source.ShouldContain("new global::Cratis.Json.EnumConverter<global::App.Outer.Kind>()");
    [Fact] void should_not_create_class_converters() => generated_source.ShouldNotContain("EnumConverter<global::App.SomeClass>");
    [Fact] void should_compile_without_errors() => errors.ShouldBeEmpty();
}
