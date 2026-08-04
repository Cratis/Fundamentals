// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { a_realm } from './given/a_realm';

describe('when a second copy is loaded', () => {
    let realm: a_realm;

    beforeEach(async () => {
        realm = new a_realm();
        (await realm.loadCopy()).registerModuleInstance();
        (await realm.loadCopy()).registerModuleInstance();
    });

    afterEach(() => realm.release());

    it('should warn exactly once', () => realm.warnings.length.should.equal(1));
    it('should name the package', () => realm.warnings[0].should.contain('@cratis/fundamentals'));
    it('should say how many copies are loaded', () => realm.warnings[0].should.contain('2 copies'));
    it('should say what goes wrong', () => realm.warnings[0].should.contain('serializes as an object instead of a string'));
    it('should say a top level pin does not reach the nested copy', () => realm.warnings[0].should.contain('does not reach a copy nested under a dependency'));
    it('should say how to collapse the copies', () => realm.warnings[0].should.contain('yarn dedupe @cratis/fundamentals'));
    it('should name where the first copy was loaded from', () => realm.warnings[0].should.contain('  1. '));
    it('should name where the second copy was loaded from', () => realm.warnings[0].should.contain('  2. '));
    it('should locate the copies rather than give up on the stack', () => realm.warnings[0].should.not.contain('unknown location'));
});
