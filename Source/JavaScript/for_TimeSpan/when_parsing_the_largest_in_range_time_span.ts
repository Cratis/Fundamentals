// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing the largest in range time span', () => {
    const parsed = TimeSpan.parse('1.23:59:59');

    it('should render back to what it was parsed from', () => parsed.toString().should.equal('1.23:59:59'));
});
