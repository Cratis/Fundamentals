// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../../JsonSerializer';
import { ModelWithDefaults } from '../fixtures/model_with_defaults';

describe('when deserializing fields with defaults with absent collections', () => {
    let result: ModelWithDefaults;

    beforeEach(() => {
        result = JsonSerializer.deserialize(ModelWithDefaults, '{}');
    });

    it('should replace an absent generic array default with an empty array', () => result.array!.should.deep.equal([]));
    it('should replace an absent enumerable default with an empty array', () => result.enumerable!.should.deep.equal([]));
});
