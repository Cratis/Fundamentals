// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { field } from '../fieldDecorator';

class UnresolvableBase {
    @field(String)
    name!: string;
}

class ContainerWithUnresolvableBase {
    @field(UnresolvableBase)
    item!: UnresolvableBase;
}

// The discriminator references a derived type that is not registered, so it cannot be resolved to a
// concrete type during deserialization — the value falls back to the declared base type.
const json = JSON.stringify({
    item: {
        name: 'kept',
        [JsonSerializer.DerivedTypeIdProperty]: 'unregistered-type-id'
    }
});

describe('when deserializing a field whose derived type cannot be resolved', () => {
    const result = JsonSerializer.deserialize(ContainerWithUnresolvableBase, json);
    const reserialized = JSON.parse(JsonSerializer.serialize(result));

    it('should fall back to the declared base type', () => result.item.constructor.should.equal(UnresolvableBase));
    it('should keep the declared field value', () => result.item.name.should.equal('kept'));
    it('should preserve the discriminator on the deserialized instance', () => (result.item as Record<string, unknown>)[JsonSerializer.DerivedTypeIdProperty].should.equal('unregistered-type-id'));
    it('should round-trip the discriminator back out on serialization', () => reserialized.item[JsonSerializer.DerivedTypeIdProperty].should.equal('unregistered-type-id'));
});
