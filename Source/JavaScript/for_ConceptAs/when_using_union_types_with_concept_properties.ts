// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { UserId, OrderCount } from './ConceptTypes';
import { field } from '../fieldDecorator';

class TestObjectWithUnionTypes {
    @field(UserId)
    userId!: UserId | string;

    @field(OrderCount)
    orderCount!: OrderCount | number;

    @field(String)
    name!: string;
}

describe('when using union types with concept properties', () => {
    describe('and assigning ConceptAs instances', () => {
        const obj = new TestObjectWithUnionTypes();
        obj.userId = new UserId('user-123');
        obj.orderCount = new OrderCount(42);
        obj.name = 'Test';

        const json = JsonSerializer.serialize(obj);
        const parsed = JSON.parse(json);

        it('should serialize userId ConceptAs instance as string value', () => parsed.userId.should.equal('user-123'));
        it('should serialize orderCount ConceptAs instance as number value', () => parsed.orderCount.should.equal(42));
        it('should serialize name as string', () => parsed.name.should.equal('Test'));

        const deserialized = JsonSerializer.deserialize(TestObjectWithUnionTypes, json);

        it('should deserialize userId as UserId instance', () => deserialized.userId.should.be.instanceOf(UserId));
        it('should deserialize userId with correct value', () => (deserialized.userId as UserId).value.should.equal('user-123'));
        it('should deserialize orderCount as OrderCount instance', () => deserialized.orderCount.should.be.instanceOf(OrderCount));
        it('should deserialize orderCount with correct value', () => (deserialized.orderCount as OrderCount).value.should.equal(42));
    });

    describe('and assigning primitive values', () => {
        const obj = new TestObjectWithUnionTypes();
        obj.userId = 'user-456';
        obj.orderCount = 99;
        obj.name = 'Test2';

        const json = JsonSerializer.serialize(obj);
        const parsed = JSON.parse(json);

        it('should serialize primitive userId as string value', () => parsed.userId.should.equal('user-456'));
        it('should serialize primitive orderCount as number value', () => parsed.orderCount.should.equal(99));
        it('should serialize name as string', () => parsed.name.should.equal('Test2'));

        const deserialized = JsonSerializer.deserialize(TestObjectWithUnionTypes, json);

        it('should deserialize primitive userId as UserId instance', () => deserialized.userId.should.be.instanceOf(UserId));
        it('should deserialize primitive userId with correct value', () => (deserialized.userId as UserId).value.should.equal('user-456'));
        it('should deserialize primitive orderCount as OrderCount instance', () => deserialized.orderCount.should.be.instanceOf(OrderCount));
        it('should deserialize primitive orderCount with correct value', () => (deserialized.orderCount as OrderCount).value.should.equal(99));
    });

    describe('and mixing ConceptAs and primitive values', () => {
        const obj = new TestObjectWithUnionTypes();
        obj.userId = new UserId('user-789');
        obj.orderCount = 55; // primitive
        obj.name = 'Test3';

        const json = JsonSerializer.serialize(obj);
        const parsed = JSON.parse(json);

        it('should serialize both types correctly', () => {
            parsed.userId.should.equal('user-789');
            parsed.orderCount.should.equal(55);
        });

        const deserialized = JsonSerializer.deserialize(TestObjectWithUnionTypes, json);

        it('should deserialize mixed types correctly', () => {
            deserialized.userId.should.be.instanceOf(UserId);
            (deserialized.userId as UserId).value.should.equal('user-789');
            deserialized.orderCount.should.be.instanceOf(OrderCount);
            (deserialized.orderCount as OrderCount).value.should.equal(55);
        });
    });
});
