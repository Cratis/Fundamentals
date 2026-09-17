// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateOnly } from '../DateOnly';

describe('when serializing it as json', () => {
    const date = DateOnly.from(2026, 8, 30);
    const serialized = JSON.stringify({ date });

    it('should render the iso string directly', () => date.toJSON().should.equal('2026-08-30'));
    it('should render the same as toString', () => date.toJSON().should.equal(date.toString()));
    it('should be a scalar string inside a serialized object', () => serialized.should.equal('{"date":"2026-08-30"}'));
});
