// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { given } from './given/given';
import { JsonSerializer } from '../JsonSerializer';
import { Guid } from '../Guid';
import { Child, Document, Enabled, Identifier, Label, Quantity, a_model_with_field_metadata } from './given/a_model_with_field_metadata';

describe('when deserializing fields with legacy decorators', given(a_model_with_field_metadata, context => {
    let result: Document;

    beforeEach(() => {
        result = JsonSerializer.deserialize(Document, JSON.stringify(context.payload));
    });

    it('should read array elements declared through generic arguments', () => result.names.should.deep.equal(['one', 'two']));
    it('should read a concept underlying Guid as a Guid', () => result.identifier.value.should.be.instanceOf(Guid));
    it('should retain the concept underlying Guid value', () => result.identifier.value.toString().should.equal(context.identifier));
    it('should wrap an empty string in its concept', () => {
        result.label.should.be.instanceOf(Label);
        result.label.value.should.equal('');
    });
    it('should wrap zero in its concept', () => {
        result.quantity.should.be.instanceOf(Quantity);
        result.quantity.value.should.equal(0);
    });
    it('should wrap false in its concept', () => {
        result.enabled.should.be.instanceOf(Enabled);
        result.enabled.value.should.equal(false);
    });
    it('should deserialize a nested model and its concept', () => {
        result.child.should.be.instanceOf(Child);
        result.child.identifier.value.should.be.instanceOf(Guid);
    });
    it('should deserialize an array of concepts', () => {
        result.identifiers[0].should.be.instanceOf(Identifier);
        result.identifiers[0].value.should.be.instanceOf(Guid);
    });
    it('should deserialize an array of nested models', () => result.children[0].should.be.instanceOf(Child));
    it('should keep reading legacy enumerable concept fields', () => result.legacyLabels[0].value.should.equal('older'));
}));
