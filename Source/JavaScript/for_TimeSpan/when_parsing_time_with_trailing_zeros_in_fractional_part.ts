// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing time with trailing zeros in fractional part', () => {
    const timeSpanString = '01:30:45.1200000';
    const parsed = TimeSpan.parse(timeSpanString);
    const convertedBack = parsed.toString();

    it('should remove trailing zeros when converting back', () => convertedBack.should.equal('01:30:45.12'));
});
