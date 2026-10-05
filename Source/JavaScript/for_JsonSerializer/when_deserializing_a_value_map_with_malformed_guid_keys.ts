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

describe('when deserializing a value map with malformed guid keys', () => {
    const plain = () => JsonSerializer.deserialize(Totals, '{"byIdentifier":{"not-a-guid":1}}');
    const quoted = () => JsonSerializer.deserialize(Totals, JSON.stringify({ byIdentifier: { '"not-a-guid"': 1 } }));

    it('should reject a malformed plain key', () => plain.should.throw('not a valid Guid'));
    it('should reject a malformed quoted key', () => quoted.should.throw('not a valid Guid'));
});
