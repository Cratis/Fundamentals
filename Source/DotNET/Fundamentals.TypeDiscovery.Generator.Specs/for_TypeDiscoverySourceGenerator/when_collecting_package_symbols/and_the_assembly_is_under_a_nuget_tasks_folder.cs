// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_collecting_package_symbols;

/// <summary>
/// Verifies that assemblies loaded from a NuGet <c>tasks/</c> folder (MSBuild task helpers
/// such as Cratis.Arc.ProxyGenerator.Build) are excluded from the generated package symbols,
/// because their DLLs are not copied to the consuming project's runtime output.
/// </summary>
public class and_the_assembly_is_under_a_nuget_tasks_folder : given.a_compilation_referencing_a_cratis_assembly
{
    void Because() => RunGeneratorWithAssemblyAt("tasks/net10.0");

    [Fact] void should_not_report_diagnostics() => _result.Diagnostics.ShouldBeEmpty();
    [Fact] void should_produce_generated_source() => _generatedSource.ShouldNotBeEmpty();
    [Fact] void should_include_the_current_assembly_type() => _generatedSource.ShouldContain("typeof(global::App.MyApp)");
    [Fact] void should_not_include_service_interface_from_build_assembly() =>
        _generatedSource.ShouldNotContain("Cratis.BuildOnly.IBuildOnlyService");
    [Fact] void should_not_include_implementation_type_from_build_assembly() =>
        _generatedSource.ShouldNotContain("Cratis.BuildOnly.BuildOnlyService");
}
