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

describe('when round tripping a value map with guid keys', () => {
    const guid = Guid.parse('f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b');
    const totals = new Totals();
    totals.byIdentifier = new ValueMap<Guid, number>().set(guid, 42);

    const written = JsonSerializer.serialize(totals);
    const read = JsonSerializer.deserialize(Totals, written);
    const legacy = JsonSerializer.deserialize(Totals, JSON.stringify({ byIdentifier: { [`"${guid}"`]: 7 } }));
    const malformed = () => JsonSerializer.deserialize(Totals, '{"byIdentifier":{"not-a-guid":1}}');
    const malformedQuoted = () => JsonSerializer.deserialize(Totals, JSON.stringify({ byIdentifier: { '"not-a-guid"': 1 } }));

    it('should write the key as the plain guid string', () => written.should.equal(`{"byIdentifier":{"${guid}":42}}`));
    it('should read the key back as a guid', () => [...read.byIdentifier.entries()][0][0].should.be.instanceof(Guid));
    it('should read the key back as the original guid', () => [...read.byIdentifier.entries()][0][0].toString().should.equal(guid.toString()));
    it('should find the value by the original key', () => read.byIdentifier.get(guid)!.should.equal(42));
    it('should read a legacy quoted key as the original guid', () => legacy.byIdentifier.get(guid)!.should.equal(7));
    it('should reject a malformed key', () => malformed.should.throw('not a valid Guid'));
    it('should reject a malformed quoted key', () => malformedQuoted.should.throw('not a valid Guid'));
});
