// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Cratis.Tasks.for_AwaitableHelpers.when_await_if_needed;


public class and_object_is_null : given.the_helper
{
    void Establish() => Input = null;
    Task Because() => AwaitIfNeeded();

    [Fact] void should_indicate_it_was_not_awaitable() => Result.IsAwaitable.ShouldBeFalse();
    [Fact] void should_return_no_result() => Result.Result.ShouldBeNull();
}
