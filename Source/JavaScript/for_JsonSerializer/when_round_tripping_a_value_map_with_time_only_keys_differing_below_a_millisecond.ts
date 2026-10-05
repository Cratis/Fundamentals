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

describe('when round tripping a value map with time only keys differing below a millisecond', () => {
    const result = JsonSerializer.deserialize(Totals, '{"byKey":{"10:11:12.1234567":1,"10:11:12.1239999":2}}');

    it('should keep both entries', () => [...result.byKey.entries()].length.should.equal(2));
    it('should find the first value', () => result.byKey.get(TimeOnly.parse('10:11:12.1234567'))!.should.equal(1));
    it('should find the second value', () => result.byKey.get(TimeOnly.parse('10:11:12.1239999'))!.should.equal(2));
});
