// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { derivedType } from '../derivedTypeDecorator';
import { field } from '../fieldDecorator';

class AutoRegisteredBase {}

@derivedType('auto-reg-deser-id')
class AutoRegisteredDerived extends AutoRegisteredBase {
    @field(Number)
    derivedValue!: number;
}

class ContainerWithAutoBase {
    @field(AutoRegisteredBase)
    item!: AutoRegisteredBase;
}

const json = JSON.stringify({
    item: {
        derivedValue: 99,
        [JsonSerializer.DerivedTypeIdProperty]: 'auto-reg-deser-id'
    }
});

describe('when deserializing field with auto-registered derived type', () => {
    const result = JsonSerializer.deserialize(ContainerWithAutoBase, json);

    it('should deserialize to the concrete derived type', () => result.item.constructor.should.equal(AutoRegisteredDerived));
    it('should have the correct value on the derived instance', () => (result.item as AutoRegisteredDerived).derivedValue.should.equal(99));
});
