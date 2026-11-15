// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeDiscoverySourceGenerator.when_a_referenced_package_type_is_removed;

public class and_the_package_no_longer_has_the_type : given.an_application_built_against_an_older_package
{
    void Because() => ReadBindings();

    [Fact] void should_not_throw() => _exception.ShouldBeNull();
    [Fact] void should_keep_the_application_self_binding() => _selfBindings.Select(type => type.FullName).ShouldContain("App.MyApp");
    [Fact] void should_keep_the_self_binding_of_a_type_the_package_still_has() => _selfBindings.Select(type => type.FullName).ShouldContain("Cratis.Pkg.Stays");
    [Fact] void should_skip_the_self_binding_of_the_removed_type() => _selfBindings.Select(type => type.FullName).ShouldNotContain("Cratis.Pkg.Gone");
    [Fact] void should_keep_the_service_binding_of_a_pair_the_package_still_has() => _serviceBindings.Select(binding => binding.Implementation.FullName).ShouldContain("Cratis.Pkg.StaysPair");
    [Fact] void should_skip_the_service_binding_of_a_removed_pair() => _serviceBindings.Select(binding => binding.Implementation.FullName).ShouldNotContain("Cratis.Pkg.GonePair");
}
