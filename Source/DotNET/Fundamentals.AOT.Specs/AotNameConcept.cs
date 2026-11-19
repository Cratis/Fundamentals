// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts;

namespace Cratis.Fundamentals.AOT.Specs;

internal sealed record AotNameConcept(string Value) : ConceptAs<string>(Value);
