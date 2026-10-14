// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../../JsonSerializer';
import { ModelWithDefaults } from '../fixtures/model_with_defaults';

describe('when deserializing fields with defaults with null collections', () => {
    let result: ModelWithDefaults;

    beforeEach(() => {
        result = JsonSerializer.deserialize(ModelWithDefaults, '{"array":null,"enumerable":null}');
    });

    it('should replace the generic array default with null', () => [result.array].should.deep.equal([null]));
    it('should replace the enumerable default with null', () => [result.enumerable].should.deep.equal([null]));
});
