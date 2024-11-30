// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../../Guid';

describe('when checking string is valid guid and it has invalid bytes', () => {
    const guidString = '437g5784-593b-477c-8685-8b52967h7ab0';
    const isValid = Guid.isGuid(guidString);

    it('should not be valid', () => isValid.should.be.false);
});
