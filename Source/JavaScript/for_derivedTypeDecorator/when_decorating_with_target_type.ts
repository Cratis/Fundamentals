// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';
import { derivedType } from '../derivedTypeDecorator';

class ExplicitBase {}

@derivedType('explicit-target-id', ExplicitBase)
class ExplicitDerived {}

describe('when decorating with target type', () => {
    it('should have registered the identifier', () => DerivedType.get(ExplicitDerived).should.equal('explicit-target-id'));
    it('should register the decorated class under the target type', () => DerivedType.getDerivedTypesFor(ExplicitBase).should.include(ExplicitDerived));
});
