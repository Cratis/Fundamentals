// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { decoratorModes, modelWithNullableFieldsFor } from './given/a_model_with_nullable_fields';

for (const mode of decoratorModes) {
    describe(`when round tripping a missing scalar field with ${mode} decorators`, () => {
        let written: object;
        let read: unknown;

        beforeEach(() => {
            const model = modelWithNullableFieldsFor(mode);
            const instance = JsonSerializer.deserialize(model, '{}');
            const json = JsonSerializer.serialize(instance);
            written = JSON.parse(json);
            read = JsonSerializer.deserialize(model, json).scalar;
        });

        it('should omit the missing field', () => written.should.not.have.property('scalar'));
        it('should read back an undefined value', () => [read].should.deep.equal([undefined]));
    });
}
