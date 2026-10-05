// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

describe('when parsing times that differ below a millisecond', () => {
    const first = TimeOnly.parse('10:11:12.1234567');
    const second = TimeOnly.parse('10:11:12.1239999');

    it('should not be equal', () => first.equals(second).should.be.false);
    it('should render the full seven digit fraction', () => first.toString().should.equal('10:11:12.1234567'));
    it('should render differently', () => first.toJSON().should.not.equal(second.toJSON()));
    it('should be equal to the same time written with trailing zeros', () => TimeOnly.parse('10:11:12.1230000').equals(TimeOnly.parse('10:11:12.123')).should.be.true);
});
