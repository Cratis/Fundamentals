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

describe('when deserializing a value map with legacy quoted DateOnly keys', () => {
    const result = JsonSerializer.deserialize(Totals, JSON.stringify({ byKey: { '"2024-03-05"': 7 } }));

    it('should read the key as the original value', () => [...result.byKey.entries()][0][0].toString().should.equal('2024-03-05'));
    it('should find the value by the original key', () => result.byKey.get(DateOnly.parse('2024-03-05'))!.should.equal(7));
});
