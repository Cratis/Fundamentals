// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.for_MeterExtensions;

public class when_beginning_scope_with_anonymous_type_using_property_name_inference : given.a_meter
{
    IMeterScope<given.a_meter> _scope;
    TestEventSequenceKey _eventSequenceKey;
    Guid _queueId;

    void Establish()
    {
        _eventSequenceKey = new TestEventSequenceKey
        {
            EventStore = "MyEventStore",
            Namespace = "MyNamespace",
            EventSequenceId = Guid.NewGuid()
        };
        _queueId = Guid.NewGuid();
    }

    void Because() => _scope = _meter.BeginScope(new
    {
        _eventSequenceKey.EventStore,
        _eventSequenceKey.Namespace,
        _eventSequenceKey.EventSequenceId,
        QueueId = _queueId
    });

    [Fact] void should_return_a_scope() => _scope.ShouldNotBeNull();
    [Fact] void should_contain_event_store_tag_with_inferred_name() => _scope.Tags["EventStore"].ShouldEqual(_eventSequenceKey.EventStore);
    [Fact] void should_contain_namespace_tag_with_inferred_name() => _scope.Tags["Namespace"].ShouldEqual(_eventSequenceKey.Namespace);
    [Fact] void should_contain_event_sequence_id_tag_with_inferred_name() => _scope.Tags["EventSequenceId"].ShouldEqual(_eventSequenceKey.EventSequenceId);
    [Fact] void should_contain_queue_id_tag() => _scope.Tags["QueueId"].ShouldEqual(_queueId);

    public class TestEventSequenceKey
    {
        public string EventStore { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public Guid EventSequenceId { get; set; }
    }
}
