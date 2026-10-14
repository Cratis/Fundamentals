// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../../JsonSerializer';
import { ModelWithSetters } from '../fixtures/model_with_setters';

describe('when deserializing fields with defaults with present setters and an absent sibling', () => {
    let result: ModelWithSetters;

    beforeEach(() => {
        result = JsonSerializer.deserialize(ModelWithSetters, '{"label":"present","quantity":3}');
    });

    it('should invoke the string setter once', () => result.label.should.equal('prefix:present'));
    it('should invoke the number setter once', () => result.quantity.should.equal(6));
    it('should keep the absent sibling default', () => [result.sibling].should.deep.equal([null]));
});
