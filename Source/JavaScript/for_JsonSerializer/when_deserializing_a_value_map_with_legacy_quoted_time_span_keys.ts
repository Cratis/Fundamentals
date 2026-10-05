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

describe('when deserializing a value map with legacy quoted time_span keys', () => {
    const result = JsonSerializer.deserialize(Totals, JSON.stringify({ byKey: { '"1.02:03:04.5"': 7 } }));

    it('should read the key as the original value', () => [...result.byKey.entries()][0][0].toString().should.equal('1.02:03:04.5'));
    it('should find the value by the original key', () => result.byKey.get(TimeSpan.parse('1.02:03:04.5'))!.should.equal(7));
});
