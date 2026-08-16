// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Fields } from '../Fields';
import { field } from '../fieldDecorator';
import { StandardFieldDecoratorContext } from '../StandardFieldDecoratorContext';

class LegacyBase { }
class StandardDerived extends LegacyBase { }
class StandardBase { }
class LegacyDerived extends StandardBase { }

describe('when legacy and standard metadata are mixed across inheritance', () => {
    const standardDerivedMetadata: Record<PropertyKey, unknown> = Object.create(null) as Record<PropertyKey, unknown>;
    const standardBaseMetadata: Record<PropertyKey, unknown> = Object.create(null) as Record<PropertyKey, unknown>;
    const contextFor = (name: string, metadata: Record<PropertyKey, unknown>): StandardFieldDecoratorContext => ({
        kind: 'field',
        metadata,
        name,
        private: false,
        static: false
    });

    field(String)(LegacyBase.prototype, 'value');
    field(String)(LegacyBase.prototype, 'legacyBaseValue');
    field(Number)(undefined, contextFor('value', standardDerivedMetadata));
    field(Number)(undefined, contextFor('standardDerivedValue', standardDerivedMetadata));
    Object.defineProperty(StandardDerived, Symbol.metadata, { value: standardDerivedMetadata });

    field(String)(undefined, contextFor('value', standardBaseMetadata));
    field(String)(undefined, contextFor('standardBaseValue', standardBaseMetadata));
    Object.defineProperty(StandardBase, Symbol.metadata, { value: standardBaseMetadata });
    field(Boolean)(LegacyDerived.prototype, 'value');
    field(Boolean)(LegacyDerived.prototype, 'legacyDerivedValue');

    const standardDerivedFields = Fields.getFieldsForType(StandardDerived);
    const legacyDerivedFields = Fields.getFieldsForType(LegacyDerived);

    it('should inherit legacy fields into a standard derived type', () => standardDerivedFields.find(_ => _.name === 'legacyBaseValue')!.type.should.equal(String));
    it('should let a standard field override a legacy base field', () => standardDerivedFields.find(_ => _.name === 'value')!.type.should.equal(Number));
    it('should inherit standard fields into a legacy derived type', () => legacyDerivedFields.find(_ => _.name === 'standardBaseValue')!.type.should.equal(String));
    it('should let a legacy field override a standard base field', () => legacyDerivedFields.find(_ => _.name === 'value')!.type.should.equal(Boolean));
});
