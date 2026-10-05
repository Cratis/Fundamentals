// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { derivedType } from '../derivedTypeDecorator';
import { field } from '../fieldDecorator';

class Base {}

@derivedType('nullable-array-derived')
class Derived extends Base {
    @field(String)
    label = 'present';
}

class Model {
    @field(Base, true)
    values!: (Base | null)[];
}

describe('when round tripping an array with a null derived element', () => {
    let written: object;
    let read: Model;

    beforeEach(() => {
        const model = new Model();
        model.values = [null, new Derived()];
        const json = JsonSerializer.serialize(model);
        written = JSON.parse(json);
        read = JsonSerializer.deserialize(Model, json);
    });

    it('should write the null element alongside the derived object', () => written.should.deep.equal({
        values: [null, { _derivedTypeId: 'nullable-array-derived', label: 'present' }]
    }));
    it('should retain the null element', () => [read.values[0]].should.deep.equal([null]));
    it('should read the non-null element as the derived type', () => read.values[1]!.should.be.instanceOf(Derived));
    it('should retain the derived field', () => (read.values[1] as Derived).label.should.equal('present'));
});
