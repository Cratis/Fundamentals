// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { ValueMap } from '../ValueMap';
import { JsonSerializer } from '../JsonSerializer';

class SimpleType {
    id: Guid = Guid.empty;
    name: string = '';
}

describe('when writing valuemap with valuemap converter', () => {
    const valueMap = new ValueMap<string, SimpleType>();
    
    const item = new SimpleType();
    item.id = Guid.parse('f47ac10b-58cc-4372-a567-0e02b2c3d479');
    item.name = 'Test';
    valueMap.set('key1', item);

    const result = JsonSerializer.serialize(valueMap);
    const parsed = JSON.parse(result);

    it('should serialize to object', () => parsed.should.be.instanceof(Object));
    it('should have key1 property', () => parsed.key1.should.not.be.undefined);
    it('should serialize name property', () => parsed.key1.name.should.equal('Test'));
    it('should serialize guid as string', () => parsed.key1.id.should.equal('f47ac10b-58cc-4372-a567-0e02b2c3d479'));
});
