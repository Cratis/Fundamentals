// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Detects that more than one copy of this package has been loaded into the same JavaScript
 * realm and reports it, once.
 *
 * `JsonSerializer` keeps its converter registry in module scope and looks types up by
 * constructor identity, so a second copy of this package brings its own registry and its own
 * `Guid` and `ConceptAs` class objects. Values then cross a boundary the serializer cannot see
 * across: a `Guid`-backed value serializes as an object instead of a string, and a version
 * pinned at the top level never reaches a copy nested under a dependency. Both failures are
 * silent, and both surface a long way from their cause.
 *
 * A package manager is free to produce that second copy, and this module does not try to stop
 * it. It makes it visible. Two installs is the usual cause, but one install reached as both ESM
 * and CommonJS is the same failure with no version mismatch anywhere to find it by - so the
 * report names where each load came from rather than only how many there were.
 *
 * A copy older than this guard does not register itself and leaves nothing behind for a newer
 * copy to find, so duplication is reported only once every copy in the realm carries the guard.
 * That is inherent to the approach, not a gap left to close.
 *
 * The bookkeeping lives on `globalThis`, because module scope is the very thing being
 * duplicated. Its two keys are a cross-version contract - a copy of a *different* version of
 * this package may have written them first - so the shapes are deliberately primitive: an
 * array of strings and a boolean.
 *
 * The registration is a load-bearing side effect. Do not declare `"sideEffects": false` in the
 * package manifest; a bundler would then be free to drop it, and the duplicate would go back
 * to being silent.
 */

/** Holds one entry per loaded copy of this package, in load order. */
const loadedInstancesKey = Symbol.for('@cratis/fundamentals.loadedInstances');

/** Records that the duplicate has been reported, so it is reported exactly once per realm. */
const duplicateReportedKey = Symbol.for('@cratis/fundamentals.duplicateReported');

const globals = globalThis as Record<symbol, unknown>;

/** Placeholder used when the runtime offers no usable stack trace. */
const unknownOrigin = 'unknown location';

/** Set once this copy has registered itself, so repeated calls count it only once. */
let registered = false;

/**
 * Resolves where this copy of the package was loaded from, so the report can name the copies
 * rather than only their number.
 * @returns {string} A source location, or a placeholder when no stack trace is available.
 */
const resolveOrigin = (): string => {
    try {
        // The first frame is this function, which lives in this copy of the package - exactly
        // the location worth naming. V8 prefixes the stack with a bare `Error` line that other
        // engines omit, so drop it rather than indexing past it.
        const frames = (new Error().stack ?? '')
            .split('\n')
            .map(line => line.trim())
            .filter(line => line.length > 0 && line !== 'Error');

        const frame = frames[0];
        if (!frame) return unknownOrigin;

        return frame.match(/\((.+)\)$/)?.[1] ?? frame.replace(/^at\s+/, '');
    } catch {
        // A runtime that refuses stack traces is not a reason to skip the registration.
        return unknownOrigin;
    }
};

/**
 * Writes the diagnostic as a single console entry naming the problem, its consequence and the
 * way out.
 * @param {string[]} origins Where each loaded copy was loaded from, in load order.
 */
const reportDuplicate = (origins: string[]): void => {
    if (typeof console === 'undefined' || typeof console.warn !== 'function') return;

    const locations = origins.map((origin, index) => `  ${index + 1}. ${origin}`).join('\n');

    console.warn(
        `[@cratis/fundamentals] Loaded ${origins.length} times into the same JavaScript realm, and it has to be loaded once.\n` +
        'Each load builds its own JSON converter registry and its own Guid and ConceptAs class objects, and every lookup is keyed ' +
        'on constructor identity. A value created by one is invisible to the other\'s serializer, so a Guid-backed value ' +
        'serializes as an object instead of a string, and a version pinned at the top level does not reach a copy nested under a ' +
        `dependency.\nLoaded from:\n${locations}\n` +
        'If those are separate installs, collapse them with `yarn dedupe @cratis/fundamentals` (or `npm dedupe`) and confirm with ' +
        '`yarn why @cratis/fundamentals`. If they differ only in dist/esm against dist/cjs it is one install reached as both ESM ' +
        'and CommonJS, which no dedupe will fix - make every importer resolve the same one.');
};

/**
 * Registers this copy of the package and reports, once per realm, when it is not the only one.
 * Calling this more than once from the same copy registers that copy once.
 */
export const registerModuleInstance = (): void => {
    if (registered) return;
    registered = true;

    const existing = globals[loadedInstancesKey];
    const origins: string[] = Array.isArray(existing) ? existing : [];
    if (origins !== existing) {
        globals[loadedInstancesKey] = origins;
    }
    origins.push(resolveOrigin());

    if (origins.length < 2 || globals[duplicateReportedKey] === true) return;

    globals[duplicateReportedKey] = true;
    reportDuplicate(origins);
};
