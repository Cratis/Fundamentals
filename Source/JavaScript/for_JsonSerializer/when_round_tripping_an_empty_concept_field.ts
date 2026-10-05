// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs } from '../ConceptAs';
import { JsonSerializer } from '../JsonSerializer';
import { decoratorModes, modelWithNullableFieldsFor } from './given/a_model_with_nullable_fields';

for (const mode of decoratorModes) {
    describe(`when round tripping an empty concept field with ${mode} decorators`, () => {
        let written: object;
        let read: ConceptAs<string>;

        beforeEach(() => {
            const model = modelWithNullableFieldsFor(mode);
            const instance = JsonSerializer.deserialize(model, '{"rich":""}');
            const json = JsonSerializer.serialize(instance);
            written = JSON.parse(json);
            read = JsonSerializer.deserialize(model, json).rich!;
        });

        it('should write the empty string without a concept wrapper', () => written.should.have.property('rich', ''));
        it('should read back a concept wrapping the empty string', () => {
            read.should.be.instanceOf(ConceptAs);
            read.value.should.equal('');
        });
    });
}
