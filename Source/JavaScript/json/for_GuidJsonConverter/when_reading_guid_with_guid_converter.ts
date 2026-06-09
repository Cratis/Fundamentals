// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';
import { GuidJsonConverter } from '../json/GuidJsonConverter';

describe('when reading guid with guid converter', () => {
    const converter = new GuidJsonConverter();
    const result = converter.read('f47ac10b-58cc-4372-a567-0e02b2c3d479');

    it('should return Guid instance', () => result.should.be.instanceof(Guid));
    it('should have correct value', () => result.toString().should.equal('f47ac10b-58cc-4372-a567-0e02b2c3d479'));
});
