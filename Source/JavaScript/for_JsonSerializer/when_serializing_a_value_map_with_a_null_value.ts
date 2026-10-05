// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';

describe('when serializing a value map with a null value', () => {
    const identifier = 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b';
    let result: object;

    beforeEach(() => {
        const values = new ValueMap<string, Guid | null>();
        values.set('missing', null);
        values.set('present', Guid.parse(identifier));
        result = JSON.parse(JsonSerializer.serialize(values));
    });

    it('should retain the null value alongside converted values', () => result.should.deep.equal({
        missing: null,
        present: identifier
    }));
});
