// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable MA0136 // Compiled C# source: line endings do not matter

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_generating_enum_converters;

public class and_an_enum_is_obsolete : given.an_enum_compilation
{
    void Because() => Generate("""
        namespace App;
        [System.Obsolete("Use something else")]
        public enum Retired { None }
        [System.Diagnostics.CodeAnalysis.Experimental("APP001")]
        public enum Preview { None }
        """);

    [Fact] void should_create_the_obsolete_enum_converter() => generated_source.ShouldContain("new global::Cratis.Json.EnumConverter<global::App.Retired>()");
    [Fact] void should_create_the_experimental_enum_converter() => generated_source.ShouldContain("new global::Cratis.Json.EnumConverter<global::App.Preview>()");
    [Fact] void should_compile_without_errors() => errors.ShouldBeEmpty();
}
