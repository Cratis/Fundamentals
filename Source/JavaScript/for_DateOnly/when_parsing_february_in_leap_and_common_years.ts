// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';

describe('when parsing february in leap and common years', () => {
    it('should accept the 29th in a leap year', () => DateOnly.parse('2024-02-29').day.should.equal(29));
    it('should accept the 29th in a year divisible by 400', () => DateOnly.parse('2000-02-29').day.should.equal(29));
    it('should reject the 29th in a common year', () => (() => DateOnly.parse('2023-02-29')).should.throw('day 29 is out of range'));
    it('should reject the 29th in a century year not divisible by 400', () => (() => DateOnly.parse('1900-02-29')).should.throw('day 29 is out of range'));
});
