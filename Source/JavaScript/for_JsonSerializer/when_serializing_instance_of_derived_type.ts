// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { derivedType } from '../derivedTypeDecorator';
import { field } from '../fieldDecorator';

@derivedType('serialize-derived-test-id')
class SerializableDerived {
    @field(Number)
    value!: number;

    @field(String)
    label!: string;
}

describe('when serializing instance of derived type', () => {
    const instance = new SerializableDerived();
    instance.value = 42;
    instance.label = 'hello';

    const result = JSON.parse(JsonSerializer.serialize(instance));

    it('should include the derived type id property', () => result[JsonSerializer.DerivedTypeIdProperty].should.equal('serialize-derived-test-id'));
    it('should include the value property', () => result.value.should.equal(42));
    it('should include the label property', () => result.label.should.equal('hello'));
});
