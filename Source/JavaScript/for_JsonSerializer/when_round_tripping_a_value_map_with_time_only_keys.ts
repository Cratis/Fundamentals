// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Totals {
    @field(ValueMap, { genericArguments: [TimeOnly, Number] })
    byKey!: ValueMap<TimeOnly, number>;
}

describe('when round tripping a value map with time_only keys', () => {
    const key = TimeOnly.parse('10:11:12.123');
    const totals = new Totals();
    totals.byKey = new ValueMap<TimeOnly, number>().set(key, 42);

    const written = JsonSerializer.serialize(totals);
    const read = JsonSerializer.deserialize(Totals, written);

    it('should write the key as the plain string', () => written.should.equal('{"byKey":{"10:11:12.123":42}}'));
    it('should read the key back as a TimeOnly', () => [...read.byKey.entries()][0][0].should.be.instanceof(TimeOnly));
    it('should read the key back as the original value', () => [...read.byKey.entries()][0][0].toString().should.equal('10:11:12.123'));
    it('should find the value by the original key', () => read.byKey.get(key)!.should.equal(42));
});
