// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { a_realm } from './given/a_realm';

describe('when one copy is loaded', () => {
    let realm: a_realm;

    beforeEach(async () => {
        realm = new a_realm();
        (await realm.loadCopy()).registerModuleInstance();
    });

    afterEach(() => realm.release());

    it('should not warn', () => realm.warnings.should.be.empty);
});
