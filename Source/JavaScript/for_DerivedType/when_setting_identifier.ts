// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';

class SomeType {}

describe('when setting identifier', () => {
    DerivedType.set(SomeType, 'some-identifier');

    it('should have set the identifier', () => DerivedType.get(SomeType).should.equal('some-identifier'));
});
