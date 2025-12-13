// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '../Guid';

describe('when constructing with undefined bytes', () => {
    const guid = new Guid(undefined);

    it('should be the same as empty', () => guid.equals(Guid.empty).should.be.true);
});
