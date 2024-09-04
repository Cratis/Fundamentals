// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';

describe('when deserializing guid', () => {
    const guidString = 'd2c7b1f3-5d5f-4d0a-8e3b-1b3f2c7e4d4d';
    const json = `"${guidString}"`;
    const result = JsonSerializer.deserialize(Guid, json);

    it('should be a guid', () => result.should.be.instanceof(Guid));
    it('should have the correct value', () => result.toString().should.equal(guidString));
});