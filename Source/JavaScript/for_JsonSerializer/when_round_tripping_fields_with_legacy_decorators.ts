// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { given } from './given/given';
import { JsonSerializer } from '../JsonSerializer';
import { Document, a_model_with_field_metadata } from './given/a_model_with_field_metadata';

describe('when round tripping fields with legacy decorators', given(a_model_with_field_metadata, context => {
    let written: {
        identifier: string;
        label: string;
        quantity: number;
        enabled: boolean;
        identifiers: string[];
        children: { identifier: string }[];
    };
    let read: Document;

    beforeEach(() => {
        const document = JsonSerializer.deserialize(Document, JSON.stringify(context.payload));
        const json = JsonSerializer.serialize(document);
        written = JSON.parse(json);
        read = JsonSerializer.deserialize(Document, json);
    });

    it('should write a concept wrapping a Guid as a string', () => written.identifier.should.equal(context.identifier));
    it('should write falsy concept payloads without object wrappers', () => [written.label, written.quantity, written.enabled].should.deep.equal(['', 0, false]));
    it('should write generic arrays of concepts as underlying values', () => written.identifiers.should.deep.equal([context.identifier]));
    it('should write generic arrays of nested models as objects', () => written.children.should.deep.equal([{ identifier: context.identifier }]));
    it('should read back the nested array model and typed concept', () => read.children[0].identifier.value.toString().should.equal(context.identifier));
}));
