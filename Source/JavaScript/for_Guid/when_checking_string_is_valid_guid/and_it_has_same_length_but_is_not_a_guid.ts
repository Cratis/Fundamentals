// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../../Guid';

describe('when checking string is valid guid and has same length but is not a guid', () => {
    const guidString = '437f5784f593bf477cf8685f8b52967f7ab0';
    const isValid = Guid.isGuid(guidString);

    it('should not be valid', () => isValid.should.be.false);
});
