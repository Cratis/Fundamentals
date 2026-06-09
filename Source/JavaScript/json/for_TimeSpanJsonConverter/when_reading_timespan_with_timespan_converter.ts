// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';
import { TimeSpanJsonConverter } from '../json/TimeSpanJsonConverter';

describe('when reading timespan with timespan converter', () => {
    const converter = new TimeSpanJsonConverter();
    const result = converter.read('1.02:30:45.678');

    it('should return TimeSpan instance', () => result.should.be.instanceof(TimeSpan));
    it('should have correct days', () => result.days.should.equal(1));
    it('should have correct hours', () => result.hours.should.equal(2));
    it('should have correct minutes', () => result.minutes.should.equal(30));
    it('should have correct seconds', () => result.seconds.should.equal(45));
});
