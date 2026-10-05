// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Cratis.Json.for_ConceptAsJsonConverter;

public class when_converting_ulong_backed_enum_concept_with_zero_value_to_json : given.converter_for_converting_to_json<ULongBackedEnumConcept, ULongBackedEnum>
{
    protected override ULongBackedEnum Expected => ULongBackedEnum.Zero;

    protected override string FormattedExpected => "0";

    void Because()
    {
        converter.Write(writer, input, default);
        writer.Flush();
        result = Encoding.UTF8.GetString(stream.ToArray());
    }

    [Fact] void should_write_the_underlying_value() => ShouldConvertToJson();
}
