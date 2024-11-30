// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../../Guid';

describe('when checking string is valid guid and it is', () => {
    const guidString = '437a5784-593b-477c-8685-8b52967b7ab0';
    const isValid = Guid.isGuid(guidString);

    it('should be valid', () => isValid.should.be.true);
});
