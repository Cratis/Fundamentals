// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { decoratorModes, modelWithNullableFieldsFor } from './given/a_model_with_nullable_fields';

for (const mode of decoratorModes) {
    describe(`when round tripping null collection fields with ${mode} decorators`, () => {
        let written: object;
        let read: unknown[];

        beforeEach(() => {
            const model = modelWithNullableFieldsFor(mode);
            const instance = JsonSerializer.deserialize(model, '{"array":null,"enumerable":null,"map":null}');
            const json = JsonSerializer.serialize(instance);
            written = JSON.parse(json);
            const roundTripped = JsonSerializer.deserialize(model, json);
            read = [roundTripped.array, roundTripped.enumerable, roundTripped.map];
        });

        it('should write null for the generic array', () => written.should.have.property('array', null));
        it('should write null for the enumerable field', () => written.should.have.property('enumerable', null));
        it('should write null for the value map', () => written.should.have.property('map', null));
        it('should read back null rather than empty collections', () => read.should.deep.equal([null, null, null]));
    });
}
