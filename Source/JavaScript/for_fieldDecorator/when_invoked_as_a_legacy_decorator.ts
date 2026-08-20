// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Fields } from '../Fields';
import { field } from '../fieldDecorator';

class FirstDerivative { }
class SecondDerivative { }
class LegacyDecoratedType { }

describe('when invoked as a legacy decorator', () => {
    field(Number)(LegacyDecoratedType.prototype, 'count');
    field(String, true, [FirstDerivative, SecondDerivative], [String, Number])(LegacyDecoratedType.prototype, 'values');

    const fields = Fields.getFieldsForType(LegacyDecoratedType);

    it('should register a primitive field', () => fields.find(_ => _.name === 'count')!.type.should.equal(Number));
    it('should preserve enumerable semantics', () => fields.find(_ => _.name === 'values')!.enumerable.should.be.true);
    it('should preserve derivative semantics', () => fields.find(_ => _.name === 'values')!.derivatives.should.deep.equal([FirstDerivative, SecondDerivative]));
    it('should preserve generic argument semantics', () => fields.find(_ => _.name === 'values')!.genericArguments.should.deep.equal([String, Number]));
});
