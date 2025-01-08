// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';
import { TopLevel, FirstDerivative, SecondDerivative } from './Types';

const json = '[{' +
'    "someNumber": 42,' +
'    "someString": "forty two",' +
'    "someDate": "2022-10-07 15:51",' +
'    "someBoolean": true,' +
'    "someGuid": "f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b",' +
'    "otherType": {' +
'       "someNumber": 43,' +
'       "someString": "forty three",' +
'       "someDate": "2022-11-07 15:51",' +
'       "someGuid": "a97f20cb-7f13-4f05-ab15-efb2b43c2457"' +
'    },' +
'    "collectionOfOtherType": [' +
'       {' +
'           "someNumber": 44,' +
'           "someString": "forty four",' +
'           "someDate": "2022-12-07 15:51",' +
'           "someGuid": "6d96dcba-3f71-4cb1-ab2c-cd6e52b7e7fd"' +
'       },' +
'       {' +
'           "someNumber": 45,' +
'           "someString": "forty five",' +
'           "someDate": "2023-01-07 15:51",' +
'           "someGuid": "3d7666c9-8929-4d41-9293-768100d0a019"' +
'       }' +
'    ],' +
'   "collectionOfDerivedTypes": [' +
'       {"firstDerivativeProperty" : 46, "_derivedTypeId": "ad7593d1-71be-4e26-9026-aedb32fc43d3" },' +
'       {"secondDerivativeProperty" : 47, "_derivedTypeId": "a038ca48-360e-46a7-8cb2-882ff21bb623" }' +
'   ]' +
'}]';


describe('when deserializing array of complex nested object with multiple wellknown types', () => {
    const result = JsonSerializer.deserializeArray(TopLevel, json);
    it('should have 1 item in the array', () => result.length.should.equal(1));
    it('should hold correct number for first level number', () => result[0].someNumber.should.equal(42));
    it('should hold correct string for first level string', () => result[0].someString.should.equal('forty two'));
    it('should hold correct bool value for first level boolean', () => result[0].someBoolean.should.be.true);
    it('should hold correct guid value for first level guid', () => result[0].someGuid.toString().should.equal('f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b'));
    it('should hold correct type for first level guid', () => result[0].someGuid.constructor.should.equal(Guid));
    it('should hold correct type for first level date', () => result[0].someDate.constructor.should.equal(Date));
    it('should hold correct value for first level date', () => result[0].someDate.toString().should.equal(new Date('2022-10-07 15:51').toString()));

    it('should hold correct number for second level number', () => result[0].otherType.someNumber.should.equal(43));
    it('should hold correct string for second level string', () => result[0].otherType.someString.should.equal('forty three'));
    it('should hold correct type for second level date', () => result[0].otherType.someDate.constructor.should.equal(Date));
    it('should hold correct guid value for second level guid', () => result[0].otherType.someGuid.toString().should.equal('a97f20cb-7f13-4f05-ab15-efb2b43c2457'));
    it('should hold correct type for second level guid', () => result[0].otherType.someGuid.constructor.should.equal(Guid));
    it('should hold correct value for second level date', () => result[0].otherType.someDate.toString().should.equal(new Date('2022-11-07 15:51').toString()));

    it('should have 2 items in the children', () => result[0].collectionOfOtherType.length.should.equal(2));
    it('should hold correct number for number in first child', () => result[0].collectionOfOtherType[0].someNumber.should.equal(44));
    it('should hold correct string for string in first child', () => result[0].collectionOfOtherType[0].someString.should.equal('forty four'));
    it('should hold correct type for date in first child', () => result[0].collectionOfOtherType[0].someDate.constructor.should.equal(Date));
    it('should hold correct value for date in first child', () => result[0].collectionOfOtherType[0].someDate.toString().should.equal(new Date('2022-12-07 15:51').toString()));
    it('should hold correct guid value for guid in first child', () => result[0].collectionOfOtherType[0].someGuid.toString().should.equal('6d96dcba-3f71-4cb1-ab2c-cd6e52b7e7fd'));

    it('should hold correct number for number in second child', () => result[0].collectionOfOtherType[1].someNumber.should.equal(45));
    it('should hold correct string for string in second child', () => result[0].collectionOfOtherType[1].someString.should.equal('forty five'));
    it('should hold correct type for date in second child', () => result[0].collectionOfOtherType[1].someDate.constructor.should.equal(Date));
    it('should hold correct value for date in second child', () => result[0].collectionOfOtherType[1].someDate.toString().should.equal(new Date('2023-01-07 15:51').toString()));
    it('should hold correct guid value for guid in second child', () => result[0].collectionOfOtherType[1].someGuid.toString().should.equal('3d7666c9-8929-4d41-9293-768100d0a019'));

    it('should have 2 items in the derived types collection', () => result[0].collectionOfDerivedTypes.length.should.equal(2));
    it('should have correct type for first derivative', () => result[0].collectionOfDerivedTypes[0].constructor.should.equal(FirstDerivative));
    it('should have correct value on first derivative', () => (result[0].collectionOfDerivedTypes[0] as FirstDerivative).firstDerivativeProperty.should.equal(46));

    it('should have correct type for second derivative', () => result[0].collectionOfDerivedTypes[1].constructor.should.equal(SecondDerivative));
    it('should have correct value on second derivative', () => (result[0].collectionOfDerivedTypes[1] as SecondDerivative).secondDerivativeProperty.should.equal(47));
});
