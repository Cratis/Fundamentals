// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Fundamentals.TypeDiscovery.Generator.Specs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_generating_convention_bindings;

public class and_implementations_have_public_constructors : Specification
{
    GeneratorDriverRunResult _result;
    string _generatedSource;
    string _registrationAttributes;

    void Because()
    {
        var compilation = CompilationFactory.CreateCompilation(
            "namespace App; " +
            "public interface IFoo {} " +
            "public class Foo : IFoo { public Foo() {} } " +
            "public class Bar {} " +
            "public class Box<T> {} " +
            "public enum Choice { None } " +
            "public static class Utilities {} " +
            "public abstract class Abstract { public Abstract() {} } " +
            "public class Hidden { private Hidden() {} } " +
            "public delegate void Callback(); " +
            "public struct ImplicitStruct {} " +
            "public struct ExplicitStruct { public ExplicitStruct() {} }");

        _result = CSharpGeneratorDriver.Create(new TypeDiscoverySourceGenerator())
            .RunGenerators(compilation)
            .GetRunResult();
        var generatedTree = _result.GeneratedTrees.Single();
        _generatedSource = generatedTree.GetText().ToString();
        var registration = generatedTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == "Register");
        _registrationAttributes = registration.AttributeLists.ToFullString();
    }

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_keep_registration_as_a_module_initializer() => _registrationAttributes.ShouldContain("[ModuleInitializer]");
    [Fact] void should_preserve_convention_implementation_constructors() => _registrationAttributes.ShouldContain(
        "[global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors, typeof(global::App.Foo))]");
    [Fact] void should_preserve_self_binding_constructors() => _registrationAttributes.ShouldContain(
        "[global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors, typeof(global::App.Bar))]");
    [Fact] void should_preserve_open_generic_constructors() => _registrationAttributes.ShouldContain(
        "[global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors, typeof(global::App.Box<>))]");
    [Fact] void should_preserve_explicit_struct_constructors() => _registrationAttributes.ShouldContain(
        "[global::System.Diagnostics.CodeAnalysis.DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors, typeof(global::App.ExplicitStruct))]");
    [Fact] void should_skip_enums() => _registrationAttributes.ShouldNotContain("typeof(global::App.Choice)");
    [Fact] void should_skip_static_classes() => _registrationAttributes.ShouldNotContain("typeof(global::App.Utilities)");
    [Fact] void should_skip_abstract_classes() => _registrationAttributes.ShouldNotContain("typeof(global::App.Abstract)");
    [Fact] void should_skip_types_with_only_non_public_constructors() => _registrationAttributes.ShouldNotContain("typeof(global::App.Hidden)");
    [Fact] void should_skip_interfaces() => _registrationAttributes.ShouldNotContain("typeof(global::App.IFoo)");
    [Fact] void should_skip_delegates() => _registrationAttributes.ShouldNotContain("typeof(global::App.Callback)");
    [Fact] void should_skip_implicit_struct_constructors_without_an_il_member() => _registrationAttributes.ShouldNotContain("typeof(global::App.ImplicitStruct)");
    [Fact] void should_keep_the_enum_binding_shape() => _generatedSource.ShouldContain(
        "new global::Cratis.DependencyInjection.ConventionSelfBinding(typeof(global::App.Choice), ");
    [Fact] void should_keep_the_non_public_constructor_binding_shape() => _generatedSource.ShouldContain(
        "new global::Cratis.DependencyInjection.ConventionSelfBinding(typeof(global::App.Hidden), ");
    [Fact] void should_keep_the_implicit_struct_binding_shape() => _generatedSource.ShouldContain(
        "new global::Cratis.DependencyInjection.ConventionSelfBinding(typeof(global::App.ImplicitStruct), ");
    [Fact] void should_keep_the_convention_binding_shape() => _generatedSource.ShouldContain(
        "new global::Cratis.DependencyInjection.ConventionServiceBinding(typeof(global::App.IFoo), typeof(global::App.Foo), ");
    [Fact] void should_keep_the_self_binding_shape() => _generatedSource.ShouldContain(
        "new global::Cratis.DependencyInjection.ConventionSelfBinding(typeof(global::App.Bar), ");
    [Fact] void should_keep_the_open_generic_binding_shape() => _generatedSource.ShouldContain(
        "new global::Cratis.DependencyInjection.ConventionSelfBinding(typeof(global::App.Box<>), ");
}
