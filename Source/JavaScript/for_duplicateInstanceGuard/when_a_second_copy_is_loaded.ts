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
    it('should say how many times it was loaded', () => realm.warnings[0].should.contain('Loaded 2 times'));
    it('should say a registered converter reaches only one copy', () => realm.warnings[0].should.contain('reaches only the copy it was registered on'));
    it('should say instanceof breaks across the copies', () => realm.warnings[0].should.contain('instanceof against the other copy'));
    it('should say a top level pin does not reach the nested copy', () => realm.warnings[0].should.contain('does not reach a copy nested under a dependency'));
    it('should say how to collapse two installs', () => realm.warnings[0].should.contain('yarn dedupe @cratis/fundamentals'));
    it('should say that dedupe is the wrong answer for one install loaded as both formats', () => realm.warnings[0].should.contain('no dedupe will fix'));
    it('should name where the first copy was loaded from', () => realm.warnings[0].should.contain('  1. '));
    it('should name where the second copy was loaded from', () => realm.warnings[0].should.contain('  2. '));
    it('should locate the copies rather than give up on the stack', () => realm.warnings[0].should.not.contain('unknown location'));
});
