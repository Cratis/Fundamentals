// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.for_TypeIdentity;

public class when_identifying_constructed_interfaces : Specification
{
    INamedTypeSymbol _open;
    INamedTypeSymbol _otherOpen;
    INamedTypeSymbol _array;
    INamedTypeSymbol _otherArray;
    INamedTypeSymbol _closed;
    INamedTypeSymbol _otherClosed;

    void Establish()
    {
        const string source = "namespace App; public interface IRepo<T> {} public class Repo<T> : IRepo<T> {} " +
            "public class CachingRepo<T> : IRepo<T> {} public class ArrayRepo<T> : IRepo<T[]> {} " +
            "public class OtherArrayRepo<T> : IRepo<T[]> {} public class ClosedRepo : IRepo<int> {} public class OtherClosedRepo : IRepo<int> {}";
        var compilation = Specs.CompilationFactory.CreateCompilation(source);
        _open = compilation.GetTypeByMetadataName("App.Repo`1").AllInterfaces.Single();
        _otherOpen = compilation.GetTypeByMetadataName("App.CachingRepo`1").AllInterfaces.Single();
        _array = compilation.GetTypeByMetadataName("App.ArrayRepo`1").AllInterfaces.Single();
        _otherArray = compilation.GetTypeByMetadataName("App.OtherArrayRepo`1").AllInterfaces.Single();
        _closed = compilation.GetTypeByMetadataName("App.ClosedRepo").AllInterfaces.Single();
        _otherClosed = compilation.GetTypeByMetadataName("App.OtherClosedRepo").AllInterfaces.Single();
    }

    [Fact]
    void should_distinguish_type_parameter_owners() => TypeIdentity.Create(_open).ShouldNotEqual(TypeIdentity.Create(_otherOpen));

    [Fact]
    void should_distinguish_owners_inside_array_arguments() => TypeIdentity.Create(_array).ShouldNotEqual(TypeIdentity.Create(_otherArray));

    [Fact]
    void should_keep_equal_closed_arguments_equal() => TypeIdentity.Create(_closed).ShouldEqual(TypeIdentity.Create(_otherClosed));
}
