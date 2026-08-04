// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { DerivedType } from '../DerivedType';
import { Fields } from '../Fields';

/**
 * Pins why `Fields` and `DerivedType` were left keyed on the constructor while the converter registry
 * was not.
 *
 * Neither holds state in module scope. Both write reflect metadata keyed on the target constructor, so
 * the state lives with the type being described rather than with the class describing it, and a
 * duplicated `Fields` reads what another `Fields` wrote. (In a real duplicate install the two copies
 * also share one metadata store, because the polyfill installs only when the global `Reflect` lacks it
 * - here a single `reflection` module is loaded, which is the same store by construction.)
 *
 * That is the whole reason those two kept comparing constructors while the converter registry stopped.
 * If it ever stops being true, this goes red and they need the same treatment the converters got.
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
