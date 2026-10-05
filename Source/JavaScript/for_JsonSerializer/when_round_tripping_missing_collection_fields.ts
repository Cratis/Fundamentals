// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { decoratorModes, modelWithNullableFieldsFor } from './given/a_model_with_nullable_fields';

for (const mode of decoratorModes) {
    describe(`when round tripping missing collection fields with ${mode} decorators`, () => {
        let written: object;
        let read: unknown[];

        beforeEach(() => {
            const model = modelWithNullableFieldsFor(mode);
            const instance = JsonSerializer.deserialize(model, '{}');
            const json = JsonSerializer.serialize(instance);
            written = JSON.parse(json);
            const roundTripped = JsonSerializer.deserialize(model, json);
            read = [roundTripped.array, roundTripped.enumerable, roundTripped.map];
        });

        it('should write empty arrays for absent array fields', () => written.should.deep.equal({ array: [], enumerable: [] }));
        it('should leave the absent value map omitted', () => written.should.not.have.property('map'));
        it('should read back empty arrays and an undefined value map', () => read.should.deep.equal([[], [], undefined]));
    });
}
