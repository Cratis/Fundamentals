// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Totals {
    @field(ValueMap, { genericArguments: [Guid, Number] })
    byIdentifier!: ValueMap<Guid, number>;
}

describe('when deserializing a value map with legacy quoted guid keys', () => {
    const guid = Guid.parse('f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b');
    const result = JsonSerializer.deserialize(Totals, JSON.stringify({ byIdentifier: { [`"${guid}"`]: 7 } }));

    it('should read the key as the original guid', () => [...result.byIdentifier.entries()][0][0].toString().should.equal(guid.toString()));
    it('should find the value by the original key', () => result.byIdentifier.get(guid)!.should.equal(7));
});
