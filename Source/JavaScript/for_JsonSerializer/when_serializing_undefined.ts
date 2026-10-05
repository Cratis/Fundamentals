// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';

describe('when serializing undefined', () => {
    let result: string | undefined;

    beforeEach(() => {
        result = JsonSerializer.serialize(undefined);
    });

    it('should preserve the JSON stringify result', () => [result].should.deep.equal([undefined]));
});
