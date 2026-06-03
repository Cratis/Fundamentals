// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';
import { UserId, OrderCount } from './ConceptTypes';
import { field } from '../fieldDecorator';

class TestObject {
    @field(UserId)
    userId!: UserId;

    @field(OrderCount)
    orderCount!: OrderCount;

    @field(String)
    name!: string;
}

describe('when deserializing object with concept properties', () => {
    const json = '{"userId":"user-456","orderCount":99,"name":"TestObject"}';
    const result = JsonSerializer.deserialize(TestObject, json);

    it('should deserialize userId as UserId instance', () => result.userId.should.be.instanceof(UserId));
    it('should have correct userId value', () => result.userId.value.should.equal('user-456'));
    it('should deserialize orderCount as OrderCount instance', () => result.orderCount.should.be.instanceof(OrderCount));
    it('should have correct orderCount value', () => result.orderCount.value.should.equal(99));
    it('should deserialize name as string', () => result.name.should.equal('TestObject'));
});
