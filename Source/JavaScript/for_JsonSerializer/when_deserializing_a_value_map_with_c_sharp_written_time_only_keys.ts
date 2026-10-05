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

describe('when deserializing a value map with TimeOnly keys as C# writes them', () => {
    const result = JsonSerializer.deserialize(Totals, JSON.stringify({ byKey: { '10:11:12.1230000': 7 } }));

    it('should read the key as the same time', () => [...result.byKey.entries()][0][0].toString().should.equal(TimeOnly.parse('10:11:12.123').toString()));
    it('should find the value by the equivalent key', () => result.byKey.get(TimeOnly.parse('10:11:12.123'))!.should.equal(7));
});
