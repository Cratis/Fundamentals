// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';
import { derivedType } from '../derivedTypeDecorator';

@derivedType('no-target-id')
class NoTargetDecorated {}

describe('when decorating without target type', () => {
    it('should have registered the identifier', () => DerivedType.get(NoTargetDecorated).should.equal('no-target-id'));
});
