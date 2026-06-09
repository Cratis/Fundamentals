// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DateJsonConverter } from '../DateJsonConverter';

describe('when writing date with date converter', () => {
    const converter = new DateJsonConverter();
    const date = new Date('2024-01-15T10:30:00.000Z');
    const result = converter.write(date);

    it('should return ISO string', () => result.should.equal('2024-01-15T10:30:00.000Z'));
});
