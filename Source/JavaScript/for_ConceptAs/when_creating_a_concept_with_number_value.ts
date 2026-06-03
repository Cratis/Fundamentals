// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { OrderCount } from '../for_JsonSerializer/Types';

describe('when creating a concept with number value', () => {
    const value = 42;
    const orderCount = new OrderCount(value);

    it('should have the correct value', () => orderCount.value.should.equal(value));
    it('should return value from valueOf', () => orderCount.valueOf().should.equal(value));
    it('should return string representation', () => orderCount.toString().should.equal('42'));
});
