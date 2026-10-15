// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns_in_context;

public class with_a_date_before_the_unix_epoch : Specification
{
    string _value;

    void Because() => _value = GenerateValue.Pattern("{yyyy}{yy}{MM}{dd}[A]", new GenerationContext(new DateTimeOffset(1, 2, 3, 0, 0, 0, TimeSpan.Zero), GenerationContext.DefaultTenant));

    [Fact] void should_format_all_date_parts_with_fixed_widths() => _value.ShouldEqual("0001010203A");
}
