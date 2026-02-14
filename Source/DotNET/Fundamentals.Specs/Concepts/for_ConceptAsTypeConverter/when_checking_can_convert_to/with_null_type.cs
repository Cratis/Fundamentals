// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptAsTypeConverter.when_checking_can_convert_to;

public class with_null_type : given.a_concept_as_type_converter
{
    bool result;

    void Because() => result = converter.CanConvertTo(null);

    [Fact] void should_return_false() => result.ShouldBeFalse();
}
