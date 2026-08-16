// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Field } from '../Field';
import { Fields } from '../Fields';
import { field } from '../fieldDecorator';

describe('when standard metadata contains unrelated fields metadata', () => {
    let fields: Field[];
    let metadata: Record<PropertyKey, unknown>;
    let unrelatedMetadata: object;

    beforeEach(() => {
        class Subject { }

        metadata = Object.create(null) as Record<PropertyKey, unknown>;
        unrelatedMetadata = { source: 'another library' };
        Reflect.defineMetadata('fields', unrelatedMetadata, metadata);

        field(String)(undefined, {
            kind: 'field',
            metadata,
            name: 'name',
            private: false,
            static: false
        });
        Object.defineProperty(Subject, Symbol.metadata, { value: metadata });
        fields = Fields.getFieldsForType(Subject);
    });

    it('should preserve the unrelated fields metadata', () => Reflect.getOwnMetadata('fields', metadata).should.equal(unrelatedMetadata));
    it('should register the decorated field independently', () => fields.map(_ => _.name).should.deep.equal(['name']));
    it('should retain the decorated field type', () => fields[0].type.should.equal(String));
});
