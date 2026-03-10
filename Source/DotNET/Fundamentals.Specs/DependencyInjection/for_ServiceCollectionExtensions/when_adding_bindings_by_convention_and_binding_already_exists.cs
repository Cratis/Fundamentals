// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_bindings_by_convention_and_binding_already_exists : Specification
{
    IServiceCollection _services;
    int _countBefore;
    int _countAfter;

    void Establish()
    {
        _services = new ServiceCollection();
        _services.AddSingleton<IMyExistingConventionBinding, MyExistingConventionBinding>();
    }

    void Because()
    {
        _countBefore = _services.Count(_ => _.ServiceType == typeof(IMyExistingConventionBinding));
        _services.AddBindingsByConvention();
        _countAfter = _services.Count(_ => _.ServiceType == typeof(IMyExistingConventionBinding));
    }

    [Fact] void should_not_add_a_duplicate_registration() => _countAfter.ShouldEqual(_countBefore);
}

public interface IMyExistingConventionBinding;

public class MyExistingConventionBinding : IMyExistingConventionBinding;
