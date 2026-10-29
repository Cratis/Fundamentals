// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v5;

public class with_the_same_inputs_twice : Specification
{
    Guid _namespace;
    string _name;
    Guid _first;
    Guid _second;
    Guid _differentNamespace;

    void Establish()
    {
        _namespace = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        _name = "www.example.com";
    }

    void Because()
    {
        _first = GenerateValue.UuidV5(_namespace, _name);
        _second = GenerateValue.UuidV5(_namespace, _name);
        _differentNamespace = GenerateValue.UuidV5(Guid.Empty, _name);
    }

    [Fact] void should_derive_the_same_uuid() => _first.ShouldEqual(_second);
    [Fact] void should_derive_a_different_uuid_for_a_different_namespace() => _differentNamespace.ShouldNotEqual(_first);
}
