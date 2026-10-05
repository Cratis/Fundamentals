// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

describe('when rendering a fraction', () => {
    it('should keep the three digit form when there is no part below the millisecond', () => TimeOnly.parse('10:11:12.1').toString().should.equal('10:11:12.100'));
    it('should pad to seven digits when there is a part below the millisecond', () => TimeOnly.parse('10:11:12.1230100').toString().should.equal('10:11:12.1230100'));
    it('should pad a short part below the millisecond', () => TimeOnly.parse('10:11:12.0000001').toString().should.equal('10:11:12.0000001'));
    it('should render the time alone when there is no fraction', () => TimeOnly.parse('10:11:12').toString().should.equal('10:11:12'));
});
