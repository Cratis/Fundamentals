// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { Document } from './given/a_model_with_field_metadata';

describe('when deserializing a generic array without a value', () => {
    it('should use an empty array for an absent collection', () => JsonSerializer.deserialize(Document, '{}').names.should.deep.equal([]));
    it('should preserve an explicit null collection', () => (JsonSerializer.deserialize(Document, '{"names":null}').names === null).should.be.true);
});
