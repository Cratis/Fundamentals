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

describe('when deserializing a value map with malformed DateOnly keys', () => {
    const plain = () => JsonSerializer.deserialize(Totals, '{"byKey":{"not-valid":1}}');
    const quoted = () => JsonSerializer.deserialize(Totals, JSON.stringify({ byKey: { '"not-valid"': 1 } }));

    it('should reject a malformed plain key', () => plain.should.throw('not a valid DateOnly'));
    it('should reject a malformed quoted key', () => quoted.should.throw('not a valid DateOnly'));
});
