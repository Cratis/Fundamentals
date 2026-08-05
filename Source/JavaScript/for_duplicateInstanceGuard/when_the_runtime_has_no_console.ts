// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { a_realm } from './given/a_realm';

/** A console that is missing the method the report writes to. */
type ConsoleWithoutWarn = { warn?: (...values: unknown[]) => void };

describe('when the runtime has no console', () => {
    let realm: a_realm;
    let error: unknown;

    beforeEach(async () => {
        realm = new a_realm();
        (console as unknown as ConsoleWithoutWarn).warn = undefined;

        try {
            (await realm.loadCopy()).registerModuleInstance();
            (await realm.loadCopy()).registerModuleInstance();
        } catch (thrown) {
            error = thrown;
        }
    });

    afterEach(() => realm.release());

    // A diagnostic that takes the process down when it has nowhere to write is worse than the silence
    // it was added to break.
    it('should not throw', () => (error === undefined).should.be.true);
});
