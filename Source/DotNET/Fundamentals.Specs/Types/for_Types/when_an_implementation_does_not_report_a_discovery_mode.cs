// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_Types;

/// <summary>
/// Pins that adding <see cref="ITypes.DiscoveryMode"/> breaks no existing implementation.
/// </summary>
/// <remarks>
/// The implementation below deliberately does not provide it. If the member were abstract this spec
/// would not compile - which is exactly what it would do in every consumer that implements the
/// interface, and what would make this a breaking change rather than an additive one.
/// </remarks>
public class when_an_implementation_does_not_report_a_discovery_mode : Specification
{
    ITypes _types;
    TypeDiscoveryMode _result;

    void Establish() => _types = new says_nothing();

    void Because() => _result = _types.DiscoveryMode;

    [Fact] void should_report_that_it_does_not_say() => _result.ShouldEqual(TypeDiscoveryMode.Unknown);

    class says_nothing : ITypes
    {
        public IEnumerable<Assembly> Assemblies => [];

        public IEnumerable<Type> All => [];

        public Type FindSingle<T>() => typeof(T);

        public IEnumerable<Type> FindMultiple<T>() => [];

        public Type FindSingle(Type type) => type;

        public IEnumerable<Type> FindMultiple(Type type) => [];

        public Type FindTypeByFullName(string fullName) => typeof(object);
    }
}
