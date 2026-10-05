// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Model {
    @field(ValueMap, { genericArguments: [String, Guid] })
    values!: ValueMap<string, Guid | null>;
}

describe('when round tripping a value map with a null Guid value', () => {
    const identifier = 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b';
    let written: object;
    let read: Model;

    beforeEach(() => {
        const model = new Model();
        model.values = new ValueMap<string, Guid | null>()
            .set('missing', null)
            .set('present', Guid.parse(identifier));
        const json = JsonSerializer.serialize(model);
        written = JSON.parse(json);
        read = JsonSerializer.deserialize(Model, json);
    });

    it('should write the null value alongside the Guid', () => written.should.deep.equal({ values: { missing: null, present: identifier } }));
    it('should retain the null entry', () => [...read.values.entries()].should.deep.equal([['missing', null], ['present', Guid.parse(identifier)]]));
    it('should read the non-null value as a Guid', () => read.values.get('present')!.should.be.instanceOf(Guid));
});
