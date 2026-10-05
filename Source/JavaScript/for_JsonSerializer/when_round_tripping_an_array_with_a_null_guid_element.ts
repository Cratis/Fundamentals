// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';
import { field } from '../fieldDecorator';

class Model {
    @field(Array, { genericArguments: [Guid] })
    values!: (Guid | null)[];
}

describe('when round tripping an array with a null Guid element', () => {
    const identifier = 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b';
    let written: object;
    let read: Model;

    beforeEach(() => {
        const model = new Model();
        model.values = [null, Guid.parse(identifier)];
        const json = JsonSerializer.serialize(model);
        written = JSON.parse(json);
        read = JsonSerializer.deserialize(Model, json);
    });

    it('should write the null element alongside the Guid', () => written.should.deep.equal({ values: [null, identifier] }));
    it('should retain the null element', () => read.values.should.deep.equal([null, Guid.parse(identifier)]));
    it('should read the non-null element as a Guid', () => read.values[1]!.should.be.instanceOf(Guid));
});
