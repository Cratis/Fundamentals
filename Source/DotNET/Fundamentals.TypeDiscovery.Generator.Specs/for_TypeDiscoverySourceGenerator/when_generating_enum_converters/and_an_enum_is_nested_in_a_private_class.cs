// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable MA0136 // Compiled C# source: line endings do not matter

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_generating_enum_converters;

public class and_an_enum_is_nested_in_a_private_class : given.an_enum_compilation
{
    void Because() => Generate("""
        namespace App;
        public class Outer
        {
            private class Hidden { public enum Kind { None } }
            protected class Protected { public enum Kind { None } }
            private protected class PrivateProtected { public enum Kind { None } }
        }
        public class Generic<T> { public enum Kind { None } }
        """);

    [Fact] void should_not_emit_enum_converters() => generated_source.ShouldNotContain("CreateEnumConverter");
    [Fact] void should_not_reference_inaccessible_enums() => generated_source.ShouldNotContain(".Kind)");
    [Fact] void should_compile_without_errors() => errors.ShouldBeEmpty();
}
