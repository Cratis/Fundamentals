// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Cratis.Tasks.for_AwaitableHelpers.when_await_if_needed;


public class and_object_is_valuetask_of_t : given.the_helper
{
    void Establish() => Input = new ValueTask<int>(123);
    Task Because() => AwaitIfNeeded();

    [Fact] void should_indicate_it_was_awaitable() => Result.IsAwaitable.ShouldBeTrue();
    [Fact] void should_return_the_result() => Result.Result.ShouldEqual(123);
}
