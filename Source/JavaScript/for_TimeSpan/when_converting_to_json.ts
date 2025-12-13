// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when converting to json', () => {
    const originalString = '3.08:45:30.5';
    const parsed = TimeSpan.parse(originalString);
    const json = parsed.toJSON();

    it('should return string format', () => json.should.equal(originalString));
});
