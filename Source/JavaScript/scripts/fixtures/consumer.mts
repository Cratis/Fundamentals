// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs, DateOnly, Guid, JsonSerializer } from '@cratis/fundamentals';
import { Point } from '@cratis/fundamentals/geospatial';
import { JsonConverter } from '@cratis/fundamentals/json';
import '@cratis/fundamentals/reflection';

const guid: Guid = Guid.empty;
const point: Point = new Point(1, 2);
const concept: typeof ConceptAs = ConceptAs;
const serializer: typeof JsonSerializer = JsonSerializer;
const converter: typeof JsonConverter = JsonConverter;
const date: typeof DateOnly = DateOnly;
void [guid, point, concept, serializer, converter, date];
