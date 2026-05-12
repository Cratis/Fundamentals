// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';

class TargetBase {}
class TargetDerived {}

describe('when setting with target type', () => {
    DerivedType.set(TargetDerived, 'target-identifier', TargetBase);

    it('should have set the identifier on the derived type', () => DerivedType.get(TargetDerived).should.equal('target-identifier'));
    it('should register the derived type under the target type', () => DerivedType.getDerivedTypesFor(TargetBase).should.include(TargetDerived));
});
