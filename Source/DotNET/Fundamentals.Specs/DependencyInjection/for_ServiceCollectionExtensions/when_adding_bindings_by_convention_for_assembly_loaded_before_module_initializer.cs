// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_bindings_by_convention_for_assembly_loaded_before_module_initializer : Specification
{
    const string _serviceTypeFullName = "Cratis.Fundamentals.Specs.ModuleInitializerProbe.IModuleInitializerProbeService";
    const string _implementationTypeFullName = "Cratis.Fundamentals.Specs.ModuleInitializerProbe.ModuleInitializerProbeService";

    IServiceCollection _services;
    ServiceDescriptor _serviceDescriptor;

    void Establish()
    {
        _services = new ServiceCollection();
        _ = Assembly.Load("Cratis.Fundamentals.Specs.ModuleInitializerProbe");
    }

    void Because()
    {
        _services.AddBindingsByConvention();
        _serviceDescriptor = _services.Single(_ => _.ServiceType.FullName == _serviceTypeFullName);
    }

    [Fact]
    void should_register_binding_from_the_loaded_assembly() =>
        _serviceDescriptor.ImplementationType?.FullName.ShouldEqual(_implementationTypeFullName);
}
