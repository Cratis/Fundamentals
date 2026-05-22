// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces.for_SpanAttribute;

public class when_created_without_an_explicit_kind : Specification
{
    SpanAttribute _attribute;

    void Because() => _attribute = new("order.process");

    [Fact] void should_default_to_internal_activity_kind() => _attribute.Kind.ShouldEqual(System.Diagnostics.ActivityKind.Internal);
}
