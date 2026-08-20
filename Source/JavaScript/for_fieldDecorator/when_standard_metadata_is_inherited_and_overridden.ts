// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Fields } from '../Fields';
import { field } from '../fieldDecorator';
import { StandardFieldDecoratorContext } from '../StandardFieldDecoratorContext';

class BaseType { }
class DerivedType extends BaseType { }

describe('when standard metadata is inherited and overridden', () => {
    const baseMetadata: Record<PropertyKey, unknown> = Object.create(null) as Record<PropertyKey, unknown>;
    const derivedMetadata: Record<PropertyKey, unknown> = Object.create(baseMetadata) as Record<PropertyKey, unknown>;
    const contextFor = (name: string, metadata: Record<PropertyKey, unknown>): StandardFieldDecoratorContext => ({
        kind: 'field',
        metadata,
        name,
        private: false,
        static: false
    });

    field(String)(undefined, contextFor('baseValue', baseMetadata));
    field(String)(undefined, contextFor('value', baseMetadata));
    field(Number)(undefined, contextFor('value', derivedMetadata));
    field(Boolean)(undefined, contextFor('derivedValue', derivedMetadata));
    Object.defineProperty(BaseType, Symbol.metadata, { value: baseMetadata });
    Object.defineProperty(DerivedType, Symbol.metadata, { value: derivedMetadata });

    const baseFields = Fields.getFieldsForType(BaseType);
    const derivedFields = Fields.getFieldsForType(DerivedType);

    it('should preserve fields declared by the base type', () => derivedFields.find(_ => _.name === 'baseValue')!.type.should.equal(String));
    it('should preserve fields declared by the derived type', () => derivedFields.find(_ => _.name === 'derivedValue')!.type.should.equal(Boolean));
    it('should keep a single overridden field', () => derivedFields.filter(_ => _.name === 'value').should.have.lengthOf(1));
    it('should use the derived field definition', () => derivedFields.find(_ => _.name === 'value')!.type.should.equal(Number));
    it('should not mutate the base field definition', () => baseFields.find(_ => _.name === 'value')!.type.should.equal(String));
});
