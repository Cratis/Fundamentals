// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Totals {
    @field(ValueMap, { genericArguments: [DateOnly, Number] })
    byKey!: ValueMap<DateOnly, number>;
}

describe('when round tripping a value map with DateOnly keys', () => {
    const key = DateOnly.parse('2024-03-05');
    const totals = new Totals();
    totals.byKey = new ValueMap<DateOnly, number>().set(key, 42);

    const written = JsonSerializer.serialize(totals);
    const read = JsonSerializer.deserialize(Totals, written);

    it('should write the key as the plain string', () => written.should.equal('{"byKey":{"2024-03-05":42}}'));
    it('should read the key back as a DateOnly', () => [...read.byKey.entries()][0][0].should.be.instanceof(DateOnly));
    it('should read the key back as the original value', () => [...read.byKey.entries()][0][0].toString().should.equal('2024-03-05'));
    it('should find the value by the original key', () => read.byKey.get(key)!.should.equal(42));
});
