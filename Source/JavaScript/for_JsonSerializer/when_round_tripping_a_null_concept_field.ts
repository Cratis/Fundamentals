// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { decoratorModes, modelWithNullableFieldsFor } from './given/a_model_with_nullable_fields';

for (const mode of decoratorModes) {
    describe(`when round tripping a null concept field with ${mode} decorators`, () => {
        let written: object;
        let read: unknown;

        beforeEach(() => {
            const model = modelWithNullableFieldsFor(mode);
            const instance = JsonSerializer.deserialize(model, '{"rich":null}');
            const json = JsonSerializer.serialize(instance);
            written = JSON.parse(json);
            read = JsonSerializer.deserialize(model, json).rich;
        });

        it('should write the explicit null without a concept wrapper', () => written.should.have.property('rich', null));
        it('should read back null rather than constructing a concept', () => [read].should.deep.equal([null]));
    });
}
