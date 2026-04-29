// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';
import { MapKeyType, TopLevel } from './Types';

const firstKey = {
    keyId: 'e094c582-9d91-4df0-b795-8d39fce089da',
    keyName: 'first'
};

const secondKey = {
    keyId: '486be95f-c860-4acd-b31e-39273b57bfc7',
    keyName: 'second'
};

const json = JSON.stringify({
    complexKeyMap: {
        [JSON.stringify(firstKey)]: {
            someNumber: 42,
            someString: 'forty two',
            someDate: '2022-10-07T15:51:00.000Z',
            someGuid: 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b'
        },
        [JSON.stringify(secondKey)]: {
            someNumber: 43,
            someString: 'forty three',
            someDate: '2022-11-07T15:51:00.000Z',
            someGuid: 'a97f20cb-7f13-4f05-ab15-efb2b43c2457'
        }
    }
});

describe('when deserializing object with value map', () => {
    const result = JsonSerializer.deserialize(TopLevel, json);

    const expectedFirstKey = new MapKeyType();
    expectedFirstKey.keyId = Guid.parse('e094c582-9d91-4df0-b795-8d39fce089da');
    expectedFirstKey.keyName = 'first';

    const expectedSecondKey = new MapKeyType();
    expectedSecondKey.keyId = Guid.parse('486be95f-c860-4acd-b31e-39273b57bfc7');
    expectedSecondKey.keyName = 'second';

    const firstValue = result.complexKeyMap.get(expectedFirstKey)!;
    const secondValue = result.complexKeyMap.get(expectedSecondKey)!;

    it('should deserialize first key and value', () => {
        firstValue.someNumber.should.equal(42);
        firstValue.someGuid.toString().should.equal('f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b');
    });

    it('should deserialize second key and value', () => {
        secondValue.someNumber.should.equal(43);
        secondValue.someGuid.toString().should.equal('a97f20cb-7f13-4f05-ab15-efb2b43c2457');
    });

    it('should deserialize map values as typed objects', () => {
        firstValue.constructor.should.not.equal(Object);
        secondValue.constructor.should.not.equal(Object);
    });
});
