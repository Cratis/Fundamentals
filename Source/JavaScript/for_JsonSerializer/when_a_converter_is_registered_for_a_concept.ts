// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ConceptAs } from '../ConceptAs';
import { Constructor } from '../Constructor';
import { JsonSerializer } from '../JsonSerializer';
import { JsonConverter } from '../json';
import { field } from '../fieldDecorator';

/* eslint-disable @typescript-eslint/no-explicit-any */

class Reference extends ConceptAs<string> {}

/** Registered for the concept itself, which is the thing that does not reach it. */
class ReferenceJsonConverter extends JsonConverter<Reference> {
    get type(): Constructor<Reference> {
        return Reference;
    }

    read(): Reference {
        return new Reference('read-by-converter');
    }

    write(): any {
        return 'written-by-converter';
    }
}

class Document {
    @field(Reference)
    reference!: Reference;
}

describe('when a converter is registered for a concept', () => {
    JsonSerializer.registerConverter(new ReferenceJsonConverter());

    const document = new Document();
    document.reference = new Reference('the-underlying-value');

    const serialized = JSON.parse(JsonSerializer.serialize(document));
    const deserialized = JsonSerializer.deserialize(Document, '{"reference":"the-underlying-value"}');

    it('should unwrap the concept rather than reach the converter on the way out', () => serialized.reference.should.equal('the-underlying-value'));
    it('should rebuild the concept rather than reach the converter on the way in', () => deserialized.reference.value.should.equal('the-underlying-value'));
    it('should still produce the concept type', () => deserialized.reference.should.be.instanceof(Reference));
});
