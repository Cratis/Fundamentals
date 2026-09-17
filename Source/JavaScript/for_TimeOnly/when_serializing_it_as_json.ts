// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

describe('when serializing it as json', () => {
    const withoutFraction = TimeOnly.from(14, 30, 45);
    const withFraction = TimeOnly.from(14, 30, 45, 123);

    it('should render the iso string directly', () => withoutFraction.toJSON().should.equal('14:30:45'));
    it('should keep the fractional part', () => withFraction.toJSON().should.equal('14:30:45.123'));
    it('should render the same as toString', () => withFraction.toJSON().should.equal(withFraction.toString()));
    it('should be a scalar string inside a serialized object', () =>
        JSON.stringify({ time: withFraction }).should.equal('{"time":"14:30:45.123"}'));
});
