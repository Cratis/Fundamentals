// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

describe('when parsing the last moment of the day', () => {
    const parsed = TimeOnly.parse('23:59:59.9999999');

    it('should keep the full fraction', () => parsed.toString().should.equal('23:59:59.9999999'));
});
