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

describe('when serializing object with concept properties', () => {
    const obj = new TestObject();
    obj.userId = new UserId('user-123');
    obj.orderCount = new OrderCount(42);
    obj.name = 'Test';

    const json = JsonSerializer.serialize(obj);
    const parsed = JSON.parse(json);

    it('should serialize userId as string value', () => parsed.userId.should.equal('user-123'));
    it('should serialize orderCount as number value', () => parsed.orderCount.should.equal(42));
    it('should serialize name as string', () => parsed.name.should.equal('Test'));
});
