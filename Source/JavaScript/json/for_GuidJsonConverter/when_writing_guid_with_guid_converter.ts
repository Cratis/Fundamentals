// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../../Guid';
import { GuidJsonConverter } from '../GuidJsonConverter';

describe('when writing guid with guid converter', () => {
    const converter = new GuidJsonConverter();
    const guid = Guid.parse('f47ac10b-58cc-4372-a567-0e02b2c3d479');
    const result = converter.write(guid);

    it('should return guid string', () => result.should.equal('f47ac10b-58cc-4372-a567-0e02b2c3d479'));
});
