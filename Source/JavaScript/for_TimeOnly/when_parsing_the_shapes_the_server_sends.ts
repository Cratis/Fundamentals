// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { TimeOnly } from '../TimeOnly';

/**
 * The server omits the seconds and the fraction when they are zero, and writes up to seven fractional digits where
 * JavaScript holds three. Every one of those shapes has to parse, and a value has to survive a parse followed by a
 * render without moving.
 */
describe('when parsing the shapes the server sends', () => {
    const withoutSeconds = TimeOnly.parse('14:30');
    const withFraction = TimeOnly.parse('14:30:45.1234567');
    const midnight = TimeOnly.parse('00:00:00');

    it('should default the seconds when they are omitted', () => withoutSeconds.second.should.equal(0));
    it('should render omitted seconds explicitly', () => withoutSeconds.toString().should.equal('14:30:00'));

    // Truncated rather than rounded, so a parse followed by a render never moves the value forward.
    it('should truncate the fraction to milliseconds', () => withFraction.millisecond.should.equal(123));
    it('should render the fraction it kept', () => withFraction.toString().should.equal('14:30:45.123'));

    it('should parse midnight', () => midnight.toString().should.equal('00:00:00'));
});
