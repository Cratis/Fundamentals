// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

// The guard registers each copy as a load-bearing side effect at module scope, so a bundler must never be told
// it is free to drop it. Declaring `"sideEffects": false` is the one manifest change that would disable
// duplicate detection without breaking anything: no build fails, no other spec notices, and a duplicated
// package quietly goes back to being invisible - which is the failure the guard exists to make visible.
//
// duplicateInstanceGuard.ts asks for this in a comment. A comment does not survive a dependency bump or a
// bundle-size pass, so this is what actually holds the line.
describe('when checking the package manifest', () => {
    const manifest = JSON.parse(
        readFileSync(join(dirname(fileURLToPath(import.meta.url)), '..', 'package.json'), 'utf-8')) as {
            sideEffects?: unknown;
        };

    // Absent is correct: a bundler then treats the package as side-effectful. An explicit `true` is fine too.
    // Only `false` is the problem, so only `false` is rejected.
    const declaresNoSideEffects = manifest.sideEffects === false;

    it('should not declare the package free of side effects', () => declaresNoSideEffects.should.be.false);
});
