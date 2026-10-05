// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';

describe('when parsing an out of range calendar date', () => {
    it('should reject a month above 12', () => (() => DateOnly.parse('2024-13-01')).should.throw('month 13 is out of range'));
    it('should reject month zero', () => (() => DateOnly.parse('2024-00-10')).should.throw('month 0 is out of range'));
    it('should reject a day beyond the month', () => (() => DateOnly.parse('2024-04-31')).should.throw('day 31 is out of range'));
    it('should reject day zero', () => (() => DateOnly.parse('2024-01-00')).should.throw('day 0 is out of range'));
    it('should reject both month and day out of range', () => (() => DateOnly.parse('2024-13-45')).should.throw('Invalid DateOnly'));
    it('should reject year zero', () => (() => DateOnly.parse('0000-01-01')).should.throw('year 0 is out of range'));
});
