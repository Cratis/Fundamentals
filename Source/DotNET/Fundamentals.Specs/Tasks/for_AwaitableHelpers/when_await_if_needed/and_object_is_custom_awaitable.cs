// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Cratis.Tasks.for_AwaitableHelpers.when_await_if_needed;


public class and_object_is_custom_awaitable : given.the_helper
{
    void Establish() => Input = new CustomAwaitable();
    Task Because() => AwaitIfNeeded();

    [Fact] void should_indicate_it_was_not_awaitable() => Result.IsAwaitable.ShouldBeFalse();
    [Fact] void should_return_no_result() => Result.Result.ShouldBeNull();

    class CustomAwaitable
    {
        public CustomAwaiter GetAwaiter() => new();
    }

    class CustomAwaiter : INotifyCompletion
    {
        public bool IsCompleted => true;
        public void OnCompleted(Action continuation) { }
        public int GetResult() => 999;
    }
}
