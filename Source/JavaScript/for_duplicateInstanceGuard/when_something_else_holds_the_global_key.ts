// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { a_realm } from './given/a_realm';

describe('when something else holds the global key', () => {
    let realm: a_realm;

    beforeEach(async () => {
        realm = new a_realm();

        // The bookkeeping is a cross-version contract, so a copy of a different version of this package
        // may have written the key first - in a shape this one does not recognize. Detection has to
        // survive that rather than take the process down over its own diagnostic.
        (globalThis as Record<symbol, unknown>)[Symbol.for('@cratis/fundamentals.loadedInstances')] = 'written by something else';

        (await realm.loadCopy()).registerModuleInstance();
        (await realm.loadCopy()).registerModuleInstance();
    });

    afterEach(() => realm.release());

    it('should still report the duplicate', () => realm.warnings.length.should.equal(1));
    it('should count only the copies it registered itself', () => realm.warnings[0].should.contain('Loaded 2 times'));
});
