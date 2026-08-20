// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { DerivedType } from '../DerivedType';
import { derivedType } from '../derivedTypeDecorator';

class BaseType { }
class OriginalType extends BaseType { }
class ReplacementType extends OriginalType { }

describe('when an outer standard decorator replaces the class', () => {
    const initializers: Array<(this: Constructor) => void> = [];

    derivedType('replacement-id')(OriginalType, {
        addInitializer(initializer) {
            initializers.push(initializer);
        },
        kind: 'class',
        metadata: {},
        name: 'OriginalType'
    });
    initializers.forEach(initializer => initializer.call(ReplacementType));

    it('should not register the discarded original class', () => (DerivedType.get(OriginalType) === undefined).should.be.true);
    it('should register the final replacement class', () => DerivedType.get(ReplacementType).should.equal('replacement-id'));
    it('should register the replacement with its base type', () => DerivedType.getDerivedTypesFor(BaseType).should.deep.equal([ReplacementType]));
});
