// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { MapKeyType, OtherType, TopLevel } from './Types';

describe('when serializing object with value map', () => {
    const topLevel = new TopLevel();
    topLevel.complexKeyMap = new ValueMap<MapKeyType, OtherType>();

    const key = new MapKeyType();
    key.keyId = Guid.parse('e094c582-9d91-4df0-b795-8d39fce089da');
    key.keyName = 'first';

    const value = new OtherType();
    value.someNumber = 42;
    value.someString = 'forty two';
    value.someDate = new Date('2022-10-07T15:51:00.000Z');
    value.someGuid = Guid.parse('f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b');

    topLevel.complexKeyMap.set(key, value);

    const result = JsonSerializer.serialize(topLevel);
    const parsed = JSON.parse(result);
    const serializedKey = JSON.stringify({
        keyId: 'e094c582-9d91-4df0-b795-8d39fce089da',
        keyName: 'first'
    });

    it('should serialize key as compact json', () => parsed.complexKeyMap[serializedKey].should.not.be.undefined);
    it('should serialize nested values', () => parsed.complexKeyMap[serializedKey].someNumber.should.equal(42));
});
