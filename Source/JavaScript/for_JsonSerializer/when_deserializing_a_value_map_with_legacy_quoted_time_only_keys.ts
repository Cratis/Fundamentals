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

describe('when deserializing a value map with legacy quoted time_only keys', () => {
    const result = JsonSerializer.deserialize(Totals, JSON.stringify({ byKey: { '"10:11:12.123"': 7 } }));

    it('should read the key as the original value', () => [...result.byKey.entries()][0][0].toString().should.equal('10:11:12.123'));
    it('should find the value by the original key', () => result.byKey.get(TimeOnly.parse('10:11:12.123'))!.should.equal(7));
});
