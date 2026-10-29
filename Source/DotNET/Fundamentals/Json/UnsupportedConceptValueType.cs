// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json;

/// <summary>
/// Exception that gets thrown when a JSON number token is read for a <see cref="Cratis.Concepts.ConceptAs{T}"/>
/// whose underlying value type is not one <see cref="ConceptAsJsonConverter{T}"/> knows how to read from a
/// number.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UnsupportedConceptValueType"/> class.
/// It derives from <see cref="System.Text.Json.JsonException"/> so serializers and hosts treat it as malformed
/// input (for example an HTTP 400 response) and attach the JSON path of the offending value.
/// </remarks>
/// <param name="conceptValueType">The underlying concept value <see cref="Type"/> that could not be read.</param>
public class UnsupportedConceptValueType(Type conceptValueType) : System.Text.Json.JsonException($"'{conceptValueType}' is not a supported underlying concept value type for a JSON number token.");
