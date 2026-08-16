// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Fields } from '../Fields';
import { field } from '../fieldDecorator';
import { StandardFieldDecoratorContext } from '../StandardFieldDecoratorContext';

class FirstDerivative { }
class SecondDerivative { }

class StandardDecoratedType {
    static instanceCount = 0;

    constructor() {
        StandardDecoratedType.instanceCount++;
    }
}

describe('when invoked as a standard decorator', () => {
    const metadata: Record<PropertyKey, unknown> = Object.create(null) as Record<PropertyKey, unknown>;
    const contextFor = (name: string): StandardFieldDecoratorContext => ({
        kind: 'field',
        metadata,
        name,
        private: false,
        static: false
    });

    field(Number)(undefined, contextFor('count'));
    field(Date)(undefined, contextFor('createdAt'));
    field(String, true)(undefined, contextFor('names'));
    field(Object, { derivatives: [FirstDerivative, SecondDerivative], genericArguments: [String, Number] })(undefined, contextFor('value'));
    Object.defineProperty(StandardDecoratedType, Symbol.metadata, { value: metadata });

    const fields = Fields.getFieldsForType(StandardDecoratedType);

    it('should expose metadata before constructing an instance', () => StandardDecoratedType.instanceCount.should.equal(0));
    it('should register a primitive field', () => fields.find(_ => _.name === 'count')!.type.should.equal(Number));
    it('should register a date field', () => fields.find(_ => _.name === 'createdAt')!.type.should.equal(Date));
    it('should register an array field', () => fields.find(_ => _.name === 'names')!.enumerable.should.be.true);
    it('should register derivatives', () => fields.find(_ => _.name === 'value')!.derivatives.should.deep.equal([FirstDerivative, SecondDerivative]));
    it('should register generic arguments', () => fields.find(_ => _.name === 'value')!.genericArguments.should.deep.equal([String, Number]));
});
