// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../../JsonSerializer';
import { ModelWithDefaults } from '../fixtures/model_with_defaults';

for (const input of [{}, { nullable: undefined, label: undefined, quantity: undefined, enabled: undefined, mirror: undefined, self: undefined, child: undefined }]) {
    describe(`when deserializing fields with defaults with ${Object.keys(input).length ? 'undefined' : 'absent'} values`, () => {
        let result: ModelWithDefaults;

        beforeEach(() => {
            result = JsonSerializer.deserializeFromInstance(ModelWithDefaults, input);
        });

        it('should keep a null initializer', () => [result.nullable].should.deep.equal([null]));
        it('should keep a string initializer', () => result.label.should.equal('default'));
        it('should keep a number initializer', () => result.quantity.should.equal(42));
        it('should keep a boolean initializer', () => result.enabled.should.be.true);
        it('should keep an object initializer by identity', () => result.mirror.should.equal(result.original));
        it('should keep a reference to the returned instance', () => result.self.should.equal(result));
        it('should leave a field without an initializer undefined', () => [result.uninitialized].should.deep.equal([undefined]));
        it('should keep an absent nested instance by identity', () => result.child.should.equal(result.originalChild));
        it('should keep the absent nested instance defaults', () => result.child.label.should.equal('child default'));
    });
}
