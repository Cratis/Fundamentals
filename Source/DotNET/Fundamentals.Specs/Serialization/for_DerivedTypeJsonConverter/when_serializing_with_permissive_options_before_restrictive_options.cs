// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Serialization.for_DerivedTypeJsonConverter;

public class when_serializing_with_permissive_options_before_restrictive_options : given.a_converter_shared_across_options
{
    void Because()
    {
        permissive_result = JsonSerializer.Serialize<IParent>(new ParentWithOptionalValue(null, 1), permissive);
        restrictive_result = JsonSerializer.Serialize<IParent>(new ParentWithOptionalValue(null, 1), restrictive);
    }

    [Fact] void should_write_null_with_permissive_options() => permissive_result.ShouldContain("\"optional\":null");
    [Fact] void should_omit_null_with_restrictive_options() => restrictive_result.ShouldNotContain("optional");
}
