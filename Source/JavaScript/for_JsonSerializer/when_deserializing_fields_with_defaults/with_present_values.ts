// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../../JsonSerializer';
import { ChildWithDefaults } from '../fixtures/child_with_defaults';
import { ModelWithDefaults } from '../fixtures/model_with_defaults';

describe('when deserializing fields with defaults with present values', () => {
    let result: ModelWithDefaults;

    beforeEach(() => {
        result = JsonSerializer.deserialize(ModelWithDefaults, JSON.stringify({
            nullable: 'present',
            label: '',
            quantity: 0,
            enabled: false,
            mirror: { label: 'present' },
            child: { label: 'present child' }
        }));
    });

    it('should override a null default', () => result.nullable!.should.equal('present'));
    it('should override a string default with an empty string', () => result.label.should.equal(''));
    it('should override a number default with zero', () => result.quantity.should.equal(0));
    it('should override a boolean default with false', () => result.enabled.should.be.false);
    it('should override an object default', () => result.mirror.should.deep.equal({ label: 'present' }));
    it('should deserialize a present nested object to its decorated type', () => result.child.should.be.instanceOf(ChildWithDefaults));
    it('should override the nested object default', () => result.child.should.not.equal(result.originalChild));
    it('should override the nested field default', () => result.child.label.should.equal('present child'));
});
