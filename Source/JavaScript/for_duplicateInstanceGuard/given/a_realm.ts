// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

const loadedInstancesKey = Symbol.for('@cratis/fundamentals.loadedInstances');
const duplicateReportedKey = Symbol.for('@cratis/fundamentals.duplicateReported');

const globals = globalThis as Record<symbol, unknown>;

/** The part of the guard's surface a spec drives. */
type PackageCopy = {
    registerModuleInstance(): void;
};

let copiesLoaded = 0;

/**
 * A realm holding no copies of the package, with `console.warn` captured.
 *
 * The guard keeps its bookkeeping on `globalThis` and its per-copy state in module scope, so a
 * spec has to control both: the global keys are saved and cleared here, and every copy is
 * loaded fresh through {@link loadCopy}.
 */
export class a_realm {
    /** Every diagnostic written while this realm is in scope. */
    readonly warnings: string[] = [];

    private readonly originalWarn = console.warn;
    private readonly savedInstances = globals[loadedInstancesKey];
    private readonly savedReported = globals[duplicateReportedKey];

    constructor() {
        delete globals[loadedInstancesKey];
        delete globals[duplicateReportedKey];
        console.warn = (...values: unknown[]) => { this.warnings.push(values.join(' ')); };
    }

    /**
     * Loads a copy of the package with its own module scope, the way a package manager
     * produces one by nesting a second physical install.
     *
     * Each call resolves a distinct module identifier, because a module that resolves to an
     * identifier already in the graph is returned from cache rather than evaluated again - and
     * an evaluation is exactly what a second copy is. The counter is module scope shared by
     * every spec file, so identifiers stay distinct across the whole run.
     *
     * `@vite-ignore` keeps the identifier out of Vite's dynamic-import-vars pass, which would
     * otherwise try to resolve the variable part by globbing the file system and fail: the
     * variation here is in the query, not in the path.
     * @returns {Promise<PackageCopy>} A copy that has not registered itself yet.
     */
    async loadCopy(): Promise<PackageCopy> {
        copiesLoaded++;
        return await import(/* @vite-ignore */ `../../duplicateInstanceGuard?copy=${copiesLoaded}`) as PackageCopy;
    }

    /** Puts back everything the realm took over, so the next spec file starts from the real state. */
    release() {
        console.warn = this.originalWarn;
        globals[loadedInstancesKey] = this.savedInstances;
        globals[duplicateReportedKey] = this.savedReported;
    }
}
