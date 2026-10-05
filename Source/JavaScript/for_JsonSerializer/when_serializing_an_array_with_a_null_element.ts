// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { JsonSerializer } from '../JsonSerializer';

describe('when serializing an array with a null element', () => {
    const identifier = 'f0fa7c5e-9f7b-4688-8851-e0b6eeebe28b';
    let result: object;

    beforeEach(() => {
        result = JSON.parse(JsonSerializer.serialize({ values: [null, Guid.parse(identifier)] }));
    });

    it('should retain the null element alongside converted values', () => result.should.deep.equal({ values: [null, identifier] }));
});
