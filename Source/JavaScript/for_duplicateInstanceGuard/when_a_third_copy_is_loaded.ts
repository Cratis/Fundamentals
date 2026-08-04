// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { a_realm } from './given/a_realm';

describe('when a third copy is loaded', () => {
    let realm: a_realm;

    beforeEach(async () => {
        realm = new a_realm();
        (await realm.loadCopy()).registerModuleInstance();
        (await realm.loadCopy()).registerModuleInstance();
        (await realm.loadCopy()).registerModuleInstance();
    });

    afterEach(() => realm.release());

    it('should still warn exactly once', () => realm.warnings.length.should.equal(1));
    it('should report the count it knew when it warned', () => realm.warnings[0].should.contain('Loaded 2 times'));
});
