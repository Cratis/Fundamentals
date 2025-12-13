// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when converting simple time to string', () => {
    const originalString = '00:30:15';
    const parsed = TimeSpan.parse(originalString);
    const convertedBack = parsed.toString();

    it('should match original string', () => convertedBack.should.equal(originalString));
});
