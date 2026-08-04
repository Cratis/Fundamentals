// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Constructor } from '../Constructor';
import { Fields } from '../Fields';
import { JsonSerializer } from '../JsonSerializer';

/**
 * Loads a module with its own scope, standing in for the second copy of the package a nested install
 * or a dual ESM/CommonJS load produces. A distinct identifier is evaluated rather than served from
 * cache, and an evaluation is what makes the class objects distinct - which is the whole problem.
 *
 * `@vite-ignore` keeps the identifier away from Vite's dynamic-import-vars pass, which would try to
 * resolve the variation by globbing the file system: the variation is in the query, not the path.
 */
const anotherCopyOf = async <TModule>(module: string): Promise<TModule> =>
    await import(/* @vite-ignore */ `../${module}?anotherCopy`) as TModule;

describe('when a type comes from another copy of the package', () => {
    let serializedGuid: { id: string };
    let serializedConcept: { reference: string };
    let deserializedConcept: { value: string };
    let deserializedValueMapKeys: unknown[];
    let guidClassIsDistinct: boolean;
    let conceptBaseIsDistinct: boolean;

    beforeEach(async () => {
        const thisCopyGuid = await import('../Guid');
        const otherGuid = await anotherCopyOf<typeof thisCopyGuid>('Guid');
        const thisCopyConceptAs = await import('../ConceptAs');
        const otherConceptAs = await anotherCopyOf<typeof thisCopyConceptAs>('ConceptAs');
        const otherValueMap = await anotherCopyOf<typeof import('../ValueMap')>('ValueMap');

        guidClassIsDistinct = thisCopyGuid.Guid !== otherGuid.Guid;
        conceptBaseIsDistinct = thisCopyConceptAs.ConceptAs !== otherConceptAs.ConceptAs;

        class Reference extends otherConceptAs.ConceptAs<string> {}

        class Holder {
            reference!: unknown;
            entries!: unknown;
        }

        Fields.addFieldToType(Holder, 'reference', Reference as unknown as Constructor, false, [], []);
        Fields.addFieldToType(Holder, 'entries', otherValueMap.ValueMap, false, [], [String, String]);

        serializedGuid = JSON.parse(JsonSerializer.serialize({ id: otherGuid.Guid.parse('a1b2c3d4-1111-2222-3333-444455556666') }));
        serializedConcept = JSON.parse(JsonSerializer.serialize({ reference: new Reference('the-value') }));

        const holder = JsonSerializer.deserialize(Holder, '{"reference":"the-value","entries":{"a":"b"}}');
        deserializedConcept = holder.reference as { value: string };
        const entries = holder.entries as InstanceType<typeof otherValueMap.ValueMap>;
        deserializedValueMapKeys = [...entries.entries()].map(entry => entry[0]);
    });

    it('should be a genuinely different Guid class', () => guidClassIsDistinct.should.be.true);
    it('should be a genuinely different ConceptAs base', () => conceptBaseIsDistinct.should.be.true);

    it('should write the other copy\'s Guid as a string', () => serializedGuid.id.should.equal('a1b2c3d4-1111-2222-3333-444455556666'));
    it('should unwrap a concept deriving from the other copy\'s ConceptAs', () => serializedConcept.reference.should.equal('the-value'));
    it('should rebuild a concept deriving from the other copy\'s ConceptAs', () => deserializedConcept.value.should.equal('the-value'));
    it('should read the other copy\'s ValueMap from its generic arguments', () => deserializedValueMapKeys.should.deep.equal(['a']));
});
