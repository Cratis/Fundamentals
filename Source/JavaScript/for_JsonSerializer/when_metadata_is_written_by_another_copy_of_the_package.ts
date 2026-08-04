// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';
import { Fields } from '../Fields';

/**
 * Pins why `Fields` and `DerivedType` were left keyed on the constructor while the converter registry
 * was not.
 *
 * Both hold their state as reflect metadata, and the polyfill that provides it installs itself only
 * when the global `Reflect` does not already have it - so a second copy of the package finds it
 * installed, skips installing its own, and the two share one store. Their class objects differ across
 * copies and it does not matter. If that ever changes, this goes red and they need the same treatment
 * the converters got.
 */
describe('when metadata is written by another copy of the package', () => {
    let fieldsAreDistinct: boolean;
    let derivedTypeIsDistinct: boolean;
    let fieldNamesReadByTheOtherCopy: string[];
    let derivedTypeReadByTheOtherCopy: string | undefined;

    beforeEach(async () => {
        const otherFields = await import(/* @vite-ignore */ '../Fields?anotherCopy') as typeof import('../Fields');
        const otherDerivedType = await import(/* @vite-ignore */ '../DerivedType?anotherCopy') as typeof import('../DerivedType');

        fieldsAreDistinct = Fields !== otherFields.Fields;
        derivedTypeIsDistinct = DerivedType !== otherDerivedType.DerivedType;

        class Subject { }

        Fields.addFieldToType(Subject, 'name', String, false, [], []);
        DerivedType.set(Subject, 'an-identifier');

        fieldNamesReadByTheOtherCopy = otherFields.Fields.getFieldsForType(Subject).map(_ => _.name);
        derivedTypeReadByTheOtherCopy = otherDerivedType.DerivedType.get(Subject);
    });

    it('should be a genuinely different Fields', () => fieldsAreDistinct.should.be.true);
    it('should be a genuinely different DerivedType', () => derivedTypeIsDistinct.should.be.true);
    it('should read a field the other copy wrote', () => fieldNamesReadByTheOtherCopy.should.deep.equal(['name']));
    it('should read a derived type the other copy wrote', () => derivedTypeReadByTheOtherCopy!.should.equal('an-identifier'));
});
