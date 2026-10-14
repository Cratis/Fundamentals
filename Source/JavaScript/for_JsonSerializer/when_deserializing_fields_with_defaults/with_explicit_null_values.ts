// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../../JsonSerializer';
import { ModelWithDefaults } from '../fixtures/model_with_defaults';

describe('when deserializing fields with defaults with explicit null values', () => {
    let result: ModelWithDefaults;

    beforeEach(() => {
        result = JsonSerializer.deserialize(ModelWithDefaults, '{"label":null,"quantity":null,"enabled":null,"mirror":null,"child":null}');
    });

    it('should override a string default with null', () => [result.label].should.deep.equal([null]));
    it('should override a number default with null', () => [result.quantity].should.deep.equal([null]));
    it('should override a boolean default with null', () => [result.enabled].should.deep.equal([null]));
    it('should override an object default with null', () => [result.mirror].should.deep.equal([null]));
    it('should override a nested instance default with null', () => [result.child].should.deep.equal([null]));
});
