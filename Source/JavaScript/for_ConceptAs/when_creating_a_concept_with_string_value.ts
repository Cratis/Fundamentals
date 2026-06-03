// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { UserId } from './ConceptTypes';

describe('when creating a concept with string value', () => {
    const value = 'user-123';
    const userId = new UserId(value);

    it('should have the correct value', () => userId.value.should.equal(value));
    it('should return value from valueOf', () => userId.valueOf().should.equal(value));
    it('should return string representation', () => userId.toString().should.equal(value));
});
