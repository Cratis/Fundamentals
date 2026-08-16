// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';
import { Field } from '../Field';
import { Fields } from '../Fields';
import { addFieldToDecoratorMetadata, getOwnDecoratorFieldsForType } from '../fieldDecoratorMetadata';

/**
 * Pins why `Fields` and `DerivedType` were left keyed on the constructor while the converter registry
 * was not.
 *
 * Neither holds state in module scope. Legacy metadata is keyed on the target constructor and standard
 * metadata is keyed on the metadata object attached to that constructor. Both live in the shared
 * Reflect metadata store, so a duplicated reader can read what another package copy wrote.
 *
 * That is the whole reason those two kept comparing constructors while the converter registry stopped.
 * If it ever stops being true, this goes red and they need the same treatment the converters got.
 */
describe('when metadata is written by another copy of the package', () => {
    let fieldsAreDistinct: boolean;
    let derivedTypeIsDistinct: boolean;
    let fieldNamesReadByTheOtherCopy: string[];
    let derivedTypeReadByTheOtherCopy: string | undefined;
    let standardFieldNamesReadByOtherFieldsCopy: string[];
    let standardFieldNamesReadByTheOtherCopy: string[];
    let standardMetadataCopiesAreDistinct: boolean;

    beforeEach(async () => {
        const otherFields = await import(/* @vite-ignore */ '../Fields?anotherCopy') as typeof import('../Fields');
        const otherDerivedType = await import(/* @vite-ignore */ '../DerivedType?anotherCopy') as typeof import('../DerivedType');
        const standardMetadataWriter = await import(/* @vite-ignore */ '../fieldDecoratorMetadata?writerCopy') as typeof import('../fieldDecoratorMetadata');
        const standardMetadataReader = await import(/* @vite-ignore */ '../fieldDecoratorMetadata?readerCopy') as typeof import('../fieldDecoratorMetadata');

        fieldsAreDistinct = Fields !== otherFields.Fields;
        derivedTypeIsDistinct = DerivedType !== otherDerivedType.DerivedType;
        standardMetadataCopiesAreDistinct = standardMetadataWriter.addFieldToDecoratorMetadata !== addFieldToDecoratorMetadata &&
            standardMetadataReader.getOwnDecoratorFieldsForType !== getOwnDecoratorFieldsForType &&
            standardMetadataWriter.addFieldToDecoratorMetadata !== standardMetadataReader.addFieldToDecoratorMetadata;

        class Subject { }

        Fields.addFieldToType(Subject, 'name', String, false, [], []);
        DerivedType.set(Subject, 'an-identifier');

        class StandardSubject { }
        const metadata: Record<PropertyKey, unknown> = Object.create(null) as Record<PropertyKey, unknown>;
        standardMetadataWriter.addFieldToDecoratorMetadata(metadata, new Field('standardName', String, false, [], []));
        Object.defineProperty(StandardSubject, Symbol.metadata, { value: metadata });

        fieldNamesReadByTheOtherCopy = otherFields.Fields.getFieldsForType(Subject).map(_ => _.name);
        derivedTypeReadByTheOtherCopy = otherDerivedType.DerivedType.get(Subject);
        standardFieldNamesReadByTheOtherCopy = standardMetadataReader.getOwnDecoratorFieldsForType(StandardSubject).map(_ => _.name);
        standardFieldNamesReadByOtherFieldsCopy = otherFields.Fields.getFieldsForType(StandardSubject).map(_ => _.name);
    });

    it('should be a genuinely different Fields', () => fieldsAreDistinct.should.be.true);
    it('should be a genuinely different DerivedType', () => derivedTypeIsDistinct.should.be.true);
    it('should use genuinely different standard metadata helpers', () => standardMetadataCopiesAreDistinct.should.be.true);
    it('should read a field the other copy wrote', () => fieldNamesReadByTheOtherCopy.should.deep.equal(['name']));
    it('should read a derived type the other copy wrote', () => derivedTypeReadByTheOtherCopy!.should.equal('an-identifier'));
    it('should read a standard field another metadata helper copy wrote', () => standardFieldNamesReadByTheOtherCopy.should.deep.equal(['standardName']));
    it('should expose standard metadata through another Fields copy', () => standardFieldNamesReadByOtherFieldsCopy.should.deep.equal(['standardName']));
});
