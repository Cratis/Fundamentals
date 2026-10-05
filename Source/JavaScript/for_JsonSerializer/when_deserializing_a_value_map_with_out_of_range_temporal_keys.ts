// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';
import { TimeOnly } from '../TimeOnly';
import { TimeSpan } from '../TimeSpan';
import { JsonSerializer } from '../JsonSerializer';
import { ValueMap } from '../ValueMap';
import { field } from '../fieldDecorator';

class Dates { @field(ValueMap, { genericArguments: [DateOnly, Number] }) byKey!: ValueMap<DateOnly, number>; }
class Times { @field(ValueMap, { genericArguments: [TimeOnly, Number] }) byKey!: ValueMap<TimeOnly, number>; }
class Spans { @field(ValueMap, { genericArguments: [TimeSpan, Number] }) byKey!: ValueMap<TimeSpan, number>; }

describe('when deserializing a value map with out of range temporal keys', () => {
    it('should reject a DateOnly key', () => (() => JsonSerializer.deserialize(Dates, '{"byKey":{"2024-13-45":1}}')).should.throw('not a valid DateOnly'));
    it('should reject a TimeOnly key', () => (() => JsonSerializer.deserialize(Times, '{"byKey":{"25:99:00":1}}')).should.throw('not a valid TimeOnly'));
    it('should reject a TimeSpan key', () => (() => JsonSerializer.deserialize(Spans, '{"byKey":{"00:60:00":1}}')).should.throw('not a valid TimeSpan'));
});
