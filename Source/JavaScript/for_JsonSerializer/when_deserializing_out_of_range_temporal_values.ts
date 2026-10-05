// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';
import { TimeOnly } from '../TimeOnly';
import { TimeSpan } from '../TimeSpan';
import { JsonSerializer } from '../JsonSerializer';
import { field } from '../fieldDecorator';

class Holder {
    @field(DateOnly) date!: DateOnly;
    @field(TimeOnly) time!: TimeOnly;
    @field(TimeSpan) span!: TimeSpan;
}

describe('when deserializing out of range temporal values', () => {
    it('should reject a DateOnly value', () => (() => JsonSerializer.deserialize(Holder, '{"date":"2024-13-45"}')).should.throw('Invalid DateOnly'));
    it('should reject a TimeOnly value', () => (() => JsonSerializer.deserialize(Holder, '{"time":"25:99:00"}')).should.throw('Invalid TimeOnly'));
    it('should reject a TimeSpan value', () => (() => JsonSerializer.deserialize(Holder, '{"span":"00:60:00"}')).should.throw('Invalid TimeSpan'));
});
