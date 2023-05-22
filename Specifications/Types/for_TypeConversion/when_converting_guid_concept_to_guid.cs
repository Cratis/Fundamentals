// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Aksio.Types.for_TypeConversion;

public class when_converting_guid_concept_to_guid : Specification
{
    GuidConcept input;
    Guid result;

    void Establish() => input = Guid.NewGuid();

    void Because() => result = (Guid)TypeConversion.Convert(typeof(Guid), input);

    [Fact] void should_convert_correctly() => result.ShouldEqual(input.Value);
}
