// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_ConceptAsJsonConverter.given;

public abstract class converter_for_converting_to_json<TConcept, TUnderlying> : Specification
{
    protected ConceptAsJsonConverter<TConcept> converter;
    protected TConcept input;
    protected string result;
    protected MemoryStream stream;
    protected Utf8JsonWriter writer;

    protected abstract TUnderlying Expected { get; }
    protected abstract string FormattedExpected { get; }

    void Establish()
    {
        converter = new();

        input = (TConcept)typeof(TConcept)
                    .GetConstructors()[0]
                    .Invoke([Expected]);
        stream = new();
        writer = new(stream);
    }

    protected void ShouldConvertToJson() => result.ShouldEqual(FormattedExpected);
}
