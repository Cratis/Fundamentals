// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_an_ignored_implementation_shares_a_convention : Specification
{
    readonly ServiceCollection _services = [];

    void Because()
    {
        // Exercise the compatibility fallback even when generated bindings exist in the spec process.
        typeof(ServiceCollectionExtensions).GetMethod("AddBindingsByConventionUsingReflectionFallback", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [_services]);
    }

    [Fact] void should_bind_the_non_ignored_implementation() => _services.Count(service => service.ServiceType == typeof(ISharedConvention) && service.ImplementationType == typeof(SharedConvention)).ShouldEqual(1);
    [Fact] void should_not_bind_an_ignored_convention() => _services.Any(service => service.ServiceType == typeof(IIgnoredConvention)).ShouldBeFalse();
    [Fact] void should_leave_genuinely_ambiguous_conventions_unbound() => _services.Any(service => service.ServiceType == typeof(IAmbiguousConvention)).ShouldBeFalse();
}

public interface ISharedConvention;
public class SharedConvention : ISharedConvention;
[IgnoreConvention]
public class IgnoredSharedConvention : ISharedConvention;

public interface IIgnoredConvention;
[IgnoreConvention]
public class IgnoredConvention : IIgnoredConvention;

public interface IAmbiguousConvention;
public class AmbiguousConvention : IAmbiguousConvention;
public class AnotherAmbiguousConvention : IAmbiguousConvention;
