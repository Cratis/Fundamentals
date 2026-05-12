// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';
import { derivedType } from '../derivedTypeDecorator';

class InheritedBase {}

@derivedType('inherited-id')
class InheritedDerived extends InheritedBase {}

describe('when decorating class that extends base class', () => {
    it('should have registered the identifier', () => DerivedType.get(InheritedDerived).should.equal('inherited-id'));
    it('should auto-register the derived class under the base class via prototype chain', () => DerivedType.getDerivedTypesFor(InheritedBase).should.include(InheritedDerived));
});
