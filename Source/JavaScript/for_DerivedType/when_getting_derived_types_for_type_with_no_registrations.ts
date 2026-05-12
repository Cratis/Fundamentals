// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';

class UnregisteredType {}

describe('when getting derived types for type with no registrations', () => {
    const result = DerivedType.getDerivedTypesFor(UnregisteredType);

    it('should return an empty array', () => result.should.be.empty);
});
