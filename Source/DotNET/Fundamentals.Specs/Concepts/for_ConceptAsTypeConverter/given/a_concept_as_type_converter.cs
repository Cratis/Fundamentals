// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_ConceptAsTypeConverter.given;

public class a_concept_as_type_converter : Specification
{
    protected ConceptAsTypeConverter<TypeConverterConcept, int> converter;

    void Establish() => converter = new();
}
