// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Fields } from '../Fields';

class OverrideBase {
}

class OverrideDerived extends OverrideBase {
}

describe('when getting fields for a derived type that overrides a base field', () => {
    Fields.addFieldToType(OverrideBase, 'value', String, false, [], []);
    Fields.addFieldToType(OverrideDerived, 'value', Number, false, [], []);

    const fields = Fields.getFieldsForType(OverrideDerived);

    it('should keep a single entry for the overridden field', () => fields.filter(_ => _.name === 'value').length.should.equal(1));
    it('should use the derived type field definition', () => fields.find(_ => _.name === 'value')!.type.should.equal(Number));
});
