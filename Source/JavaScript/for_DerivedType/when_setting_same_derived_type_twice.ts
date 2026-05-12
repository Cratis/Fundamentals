// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';

class DuplicateBase {}
class DuplicateDerived {}

describe('when setting same derived type twice', () => {
    DerivedType.set(DuplicateDerived, 'dup-id', DuplicateBase);
    DerivedType.set(DuplicateDerived, 'dup-id', DuplicateBase);

    it('should register the derived type only once', () => DerivedType.getDerivedTypesFor(DuplicateBase).should.have.lengthOf(1));
});
