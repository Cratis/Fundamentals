// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeSpan } from '../TimeSpan';

describe('when parsing out of range time', () => {
    it('should reject minutes above 59', () => (() => TimeSpan.parse('00:60:00')).should.throw('minutes 60 is out of range'));
    it('should reject seconds above 59', () => (() => TimeSpan.parse('00:00:60')).should.throw('seconds 60 is out of range'));
    it('should reject hours above 23', () => (() => TimeSpan.parse('1.24:00:00')).should.throw('hours 24 is out of range'));
    it('should reject hours above 23 without days', () => (() => TimeSpan.parse('25:00:00')).should.throw('hours 25 is out of range'));
});
