// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

describe('when parsing an out of range time of day', () => {
    it('should reject an hour above 23', () => (() => TimeOnly.parse('24:00:00')).should.throw('hour 24 is out of range'));
    it('should reject a minute above 59', () => (() => TimeOnly.parse('10:60:00')).should.throw('minute 60 is out of range'));
    it('should reject a second above 59', () => (() => TimeOnly.parse('10:00:60')).should.throw('second 60 is out of range'));
    it('should reject every component out of range', () => (() => TimeOnly.parse('25:99:00')).should.throw('Invalid TimeOnly'));
});
