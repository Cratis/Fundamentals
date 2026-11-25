// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Types.for_InstancesOf;

public class when_enumerating_lazily_with_a_container : Specification
{
    ServiceProvider provider;
    IAmAnInterface[] first;
    IAmAnInterface[] second;
    Exception exception_on_creating;
    Exception exception_on_first_step;
    int resolutions_before_enumeration;
    int resolutions;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddTransient<OneImplementation>(_ =>
        {
            resolutions++;
            return new OneImplementation();
        });
        services.AddTransient<SecondImplementation>();
        provider = services.BuildServiceProvider();
    }

    void Because()
    {
        var types = new Mock<ITypes>();
        types.Setup(t => t.FindMultiple<IAmAnInterface>()).Returns([typeof(OneImplementation), typeof(SecondImplementation)]);
        var instances = new InstancesOf<IAmAnInterface>(types.Object, provider);

        IEnumerator<IAmAnInterface> enumerator = null;
        exception_on_creating = Catch.Exception(() => enumerator = instances.GetEnumerator());
        resolutions_before_enumeration = resolutions;
        exception_on_first_step = Catch.Exception(() => enumerator.MoveNext());
        first = [.. instances];
        second = [.. instances];
    }

    void Destroy() => provider.Dispose();

    [Fact] void should_not_fail_when_creating_the_enumerator() => exception_on_creating.ShouldBeNull();
    [Fact] void should_not_resolve_before_enumeration_starts() => resolutions_before_enumeration.ShouldEqual(0);
    [Fact] void should_resolve_when_enumeration_starts() => exception_on_first_step.ShouldBeNull();
    [Fact] void should_keep_discovery_order() => first.Select(_ => _.GetType()).ShouldContainOnly(typeof(OneImplementation), typeof(SecondImplementation));
    [Fact] void should_resolve_again_for_each_enumeration() => second.Intersect(first).ShouldBeEmpty();
}
