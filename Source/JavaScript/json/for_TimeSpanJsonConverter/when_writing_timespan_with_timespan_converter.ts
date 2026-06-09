// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../../TimeSpan';
import { TimeSpanJsonConverter } from '../TimeSpanJsonConverter';

describe('when writing timespan with timespan converter', () => {
    const converter = new TimeSpanJsonConverter();
    const timespan = TimeSpan.parse('1.02:30:45.678');
    const result = converter.write(timespan);

    it('should return timespan string', () => result.should.equal('1.02:30:45.678'));
});
