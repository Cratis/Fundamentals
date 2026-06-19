// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Fields } from '../Fields';

class BaseType {
}

class MiddleType extends BaseType {
}

class DerivedType extends MiddleType {
}

describe('when getting fields for derived type', () => {
    Fields.addFieldToType(BaseType, 'baseField', String, false, [], []);
    Fields.addFieldToType(MiddleType, 'middleField', Number, false, [], []);
    Fields.addFieldToType(DerivedType, 'derivedField', Boolean, false, [], []);

    const fields = Fields.getFieldsForType(DerivedType);
    const names = fields.map(_ => _.name);

    it('should include the field declared on the derived type', () => names.should.contain('derivedField'));
    it('should include the field inherited from the middle type', () => names.should.contain('middleField'));
    it('should include the field inherited from the base type', () => names.should.contain('baseField'));
    it('should include all three fields', () => fields.length.should.equal(3));
});
