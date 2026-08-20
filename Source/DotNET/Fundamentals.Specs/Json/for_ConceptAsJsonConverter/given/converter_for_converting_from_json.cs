// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Json.for_ConceptAsJsonConverter.given;

public abstract class converter_for_converting_from_json<TConcept, TUnderlying> : Specification
{
    protected ConceptAsJsonConverter<TConcept> converter;
    protected TUnderlying input;
    protected TConcept result;

    protected abstract TUnderlying InputValue { get; }
    protected abstract string FormattedInput { get; }
    protected Type ConceptType => typeof(TConcept);

    void Establish()
    {
        converter = new();
        input = InputValue;
    }

    protected void ShouldConvertToCorrectConcept() => result.GetConceptValue().ShouldEqual(input);
}
