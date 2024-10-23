// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { Guid } from '../Guid';
import { OtherType, TopLevel } from './Types';

describe('when serializing complex nested object with multiple wellknown types', () => {
    const instance = new TopLevel();
    instance.someNumber = 42;
    instance.someString = 'forty two';
    instance.someDate = new Date('2022-10-07 15:51');
    instance.someBoolean = true;
    instance.collectionOfGuids = [Guid.create(), Guid.create()];
    
    instance.someGuid = Guid.parse('f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b');
    instance.otherType = new OtherType();
    instance.otherType.someNumber = 43;
    instance.otherType.someString = 'forty three';
    instance.otherType.someDate = new Date('2022-11-07 15:51');
    instance.otherType.someGuid = Guid.parse('a97f20cb-7f13-4f05-ab15-efb2b43c2457');
    instance.collectionOfOtherType = [
        new OtherType(),
        new OtherType()
    ];
    instance.collectionOfOtherType[0].someNumber = 44;
    instance.collectionOfOtherType[0].someString = 'forty four';
    instance.collectionOfOtherType[0].someGuid = Guid.parse('b8987c5c-6506-4da7-8f19-a50cfc218c54');
    instance.collectionOfOtherType[0].someDate = new Date('2022-12-07 15:51');
    instance.collectionOfOtherType[1].someNumber = 45;
    instance.collectionOfOtherType[1].someString = 'forty five';
    instance.collectionOfOtherType[1].someGuid = Guid.parse('ee503799-63a7-4666-84b8-46eb1641206a');
    instance.collectionOfOtherType[1].someDate = new Date('2023-01-07 15:51');
   
    const result = JsonSerializer.serialize(instance);
    const deserialized = JSON.parse(result);

    it('should hold correct number for first level number', () => deserialized.someNumber.should.equal(42));
    it('should hold correct string for first level string', () => deserialized.someString.should.equal('forty two'));
    it('should hold correct bool value for first level boolean', () => deserialized.someBoolean.should.be.true);
    it('should hold correct guid value for first level guid', () => deserialized.someGuid.should.equal(instance.someGuid.toString()));
    it('should hold correct guid value for first guid in the first level guid array', () => deserialized.collectionOfGuids[0].should.equal(instance.collectionOfGuids[0].toString()));
    it('should hold correct guid value for second guid in the first level guid array', () => deserialized.collectionOfGuids[1].should.equal(instance.collectionOfGuids[1].toString()));
    it('should hold correct value for first level date', () => deserialized.someDate.toString().should.equal(JSON.stringify(instance.someDate).slice(1, -1)));

    it('should hold correct number for second level number', () => deserialized.otherType.someNumber.should.equal(43));
    it('should hold correct string for second level string', () => deserialized.otherType.someString.should.equal('forty three'));
    it('should hold correct guid value for second level guid', () => deserialized.otherType.someGuid.should.equal(instance.otherType.someGuid.toString()));
    it('should hold correct value for second level date', () => deserialized.otherType.someDate.toString().should.equal(JSON.stringify(instance.otherType.someDate).slice(1, -1)));

    it('should have 2 items in the children', () => deserialized.collectionOfOtherType.length.should.equal(2));
    it('should hold correct number for number in first child', () => deserialized.collectionOfOtherType[0].someNumber.should.equal(44));
    it('should hold correct string for string in first child', () => deserialized.collectionOfOtherType[0].someString.should.equal('forty four'));
    it('should hold correct value for date in first child', () => deserialized.collectionOfOtherType[0].someDate.toString().should.equal(JSON.stringify(instance.collectionOfOtherType[0].someDate).slice(1, -1)));
    it('should hold correct guid value for guid in first child', () => deserialized.collectionOfOtherType[0].someGuid.toString().should.equal(instance.collectionOfOtherType[0].someGuid.toString()));

    it('should hold correct number for number in second child', () => deserialized.collectionOfOtherType[1].someNumber.should.equal(45));
    it('should hold correct string for string in second child', () => deserialized.collectionOfOtherType[1].someString.should.equal('forty five'));
    it('should hold correct value for date in second child', () => deserialized.collectionOfOtherType[1].someDate.toString().should.equal(JSON.stringify(instance.collectionOfOtherType[1].someDate).slice(1, -1)));
    it('should hold correct guid value for guid in second child', () => deserialized.collectionOfOtherType[1].someGuid.toString().should.equal(instance.collectionOfOtherType[1].someGuid.toString()));
});