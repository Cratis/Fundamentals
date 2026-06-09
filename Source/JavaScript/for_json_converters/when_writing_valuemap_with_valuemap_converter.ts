// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { ValueMap } from '../ValueMap';
import { ValueMapJsonConverter } from '../json/ValueMapJsonConverter';

class SimpleType {
    id: Guid = Guid.empty;
    name: string = '';
}

describe('when writing valuemap with valuemap converter', () => {
    const converter = new ValueMapJsonConverter();
    const valueMap = new ValueMap<string, SimpleType>();
    
    const item = new SimpleType();
    item.id = Guid.parse('f47ac10b-58cc-4372-a567-0e02b2c3d479');
    item.name = 'Test';
    valueMap.set('key1', item);

    const result = converter.write(valueMap);

    it('should return object', () => result.should.be.instanceof(Object));
    it('should have key1 property', () => result.key1.should.not.be.undefined);
    it('should have serialized value', () => result.key1.name.should.equal('Test'));
});
