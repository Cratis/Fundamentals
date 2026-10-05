// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs } from '../ConceptAs';
import { JsonSerializer } from '../JsonSerializer';
import { field } from '../fieldDecorator';

class Label extends ConceptAs<string> {}

class Model {
    @field(Array, { genericArguments: [Label] })
    values!: (Label | null)[];
}

describe('when round tripping an array with a null concept element', () => {
    let written: object;
    let read: Model;

    beforeEach(() => {
        const model = new Model();
        model.values = [null, new Label('present')];
        const json = JsonSerializer.serialize(model);
        written = JSON.parse(json);
        read = JsonSerializer.deserialize(Model, json);
    });

    it('should write the null element alongside the underlying value', () => written.should.deep.equal({ values: [null, 'present'] }));
    it('should retain the null element rather than construct a concept', () => read.values.should.deep.equal([null, new Label('present')]));
    it('should read the non-null element as a concept', () => read.values[1]!.should.be.instanceOf(Label));
});
