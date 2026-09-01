// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_bindings_to_two_separate_collections : Specification
{
    IServiceCollection _firstCollection;
    IServiceCollection _secondCollection;
    (Type ServiceType, Type? ImplementationType, ServiceLifetime Lifetime)[] _firstCollectionBindings;
    (Type ServiceType, Type? ImplementationType, ServiceLifetime Lifetime)[] _secondCollectionBindings;

    void Establish()
    {
        _firstCollection = new ServiceCollection();
        _secondCollection = new ServiceCollection();
    }

    void Because()
    {
        _firstCollection.AddBindingsByConvention();
        _firstCollection.AddSelfBindings();
        _secondCollection.AddBindingsByConvention();
        _secondCollection.AddSelfBindings();

        // The replay onto the second collection is asserted as a subset rather than sequence equality -
        // the generated provider registry is process-global and only grows, and other specs register
        // providers concurrently, so the second collection may legitimately pick up more bindings. What
        // must hold is that nothing the first collection received goes missing on a later collection.
        _firstCollectionBindings = [.. _firstCollection.Select(_ => (_.ServiceType, _.ImplementationType, _.Lifetime))];
        _secondCollectionBindings = [.. _secondCollection.Select(_ => (_.ServiceType, _.ImplementationType, _.Lifetime))];
    }

    [Fact] void should_add_registrations_to_the_first_collection() => _firstCollectionBindings.ShouldNotBeEmpty();

    [Fact] void should_replay_every_registration_from_the_first_collection_onto_the_second() =>
        _firstCollectionBindings.Except(_secondCollectionBindings).ShouldBeEmpty();
}
