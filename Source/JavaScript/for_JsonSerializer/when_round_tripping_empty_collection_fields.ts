// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { decoratorModes, modelWithNullableFieldsFor } from './given/a_model_with_nullable_fields';

for (const mode of decoratorModes) {
    describe(`when round tripping empty collection fields with ${mode} decorators`, () => {
        let written: object;
        let readArrays: unknown[];
        let readMap: ValueMap<string, string>;

        beforeEach(() => {
            const model = modelWithNullableFieldsFor(mode);
            const instance = JsonSerializer.deserialize(model, '{"array":[],"enumerable":[],"map":{}}');
            const json = JsonSerializer.serialize(instance);
            written = JSON.parse(json);
            const roundTripped = JsonSerializer.deserialize(model, json);
            readArrays = [roundTripped.array, roundTripped.enumerable];
            readMap = roundTripped.map!;
        });

        it('should write the empty collections', () => written.should.deep.equal({ array: [], enumerable: [], map: {} }));
        it('should read back empty arrays', () => readArrays.should.deep.equal([[], []]));
        it('should read back an empty value map', () => {
            readMap.should.be.instanceOf(ValueMap);
            [...readMap.entries()].should.be.empty;
        });
    });
}
