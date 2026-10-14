// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../../JsonSerializer';
import { ChildWithDefaults } from '../fixtures/child_with_defaults';
import { ModelWithDefaults } from '../fixtures/model_with_defaults';

describe('when deserializing fields with defaults with a nested object without values', () => {
    let result: ModelWithDefaults;

    beforeEach(() => {
        result = JsonSerializer.deserialize(ModelWithDefaults, '{"child":{}}');
    });

    it('should deserialize a present nested object to its decorated type', () => result.child.should.be.instanceOf(ChildWithDefaults));
    it('should replace the parent default instance', () => result.child.should.not.equal(result.originalChild));
    it('should keep the absent nested field default', () => result.child.label.should.equal('child default'));
});
