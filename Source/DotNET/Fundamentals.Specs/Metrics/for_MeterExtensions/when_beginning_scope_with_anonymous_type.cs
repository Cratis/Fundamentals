// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.for_MeterExtensions;

public class when_beginning_scope_with_anonymous_type : given.a_meter
{
    IMeterScope<given.a_meter> _scope;
    string _eventStore;
    string _namespace;
    Guid _eventSequenceId;
    int _queueId;

    void Establish()
    {
        _eventStore = "MyEventStore";
        _namespace = "MyNamespace";
        _eventSequenceId = Guid.NewGuid();
        _queueId = 42;
    }

    void Because() => _scope = _meter.BeginScope(new
    {
        EventStore = _eventStore,
        Namespace = _namespace,
        EventSequenceId = _eventSequenceId,
        QueueId = _queueId
    });

    [Fact] void should_return_a_scope() => _scope.ShouldNotBeNull();
    [Fact] void should_contain_event_store_tag() => _scope.Tags["EventStore"].ShouldEqual(_eventStore);
    [Fact] void should_contain_namespace_tag() => _scope.Tags["Namespace"].ShouldEqual(_namespace);
    [Fact] void should_contain_event_sequence_id_tag() => _scope.Tags["EventSequenceId"].ShouldEqual(_eventSequenceId);
    [Fact] void should_contain_queue_id_tag() => _scope.Tags["QueueId"].ShouldEqual(_queueId);
}
