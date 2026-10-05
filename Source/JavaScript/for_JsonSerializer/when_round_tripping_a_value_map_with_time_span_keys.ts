// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Totals {
    @field(ValueMap, { genericArguments: [TimeSpan, Number] })
    byKey!: ValueMap<TimeSpan, number>;
}

describe('when round tripping a value map with TimeSpan keys', () => {
    const key = TimeSpan.parse('1.02:03:04.5');
    const totals = new Totals();
    totals.byKey = new ValueMap<TimeSpan, number>().set(key, 42);

    const written = JsonSerializer.serialize(totals);
    const read = JsonSerializer.deserialize(Totals, written);

    it('should write the key as the plain string', () => written.should.equal('{"byKey":{"1.02:03:04.5":42}}'));
    it('should read the key back as a TimeSpan', () => [...read.byKey.entries()][0][0].should.be.instanceof(TimeSpan));
    it('should read the key back as the original value', () => [...read.byKey.entries()][0][0].toString().should.equal('1.02:03:04.5'));
    it('should find the value by the original key', () => read.byKey.get(key)!.should.equal(42));
});
