// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { JsonSerializer } from '../JsonSerializer';

describe('when serializing null', () => {
    let result: string;

    beforeEach(() => {
        result = JsonSerializer.serialize(null);
    });

    it('should write the JSON null literal', () => result.should.equal('null'));
});
