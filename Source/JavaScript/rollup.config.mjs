// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import typescript2 from 'rollup-plugin-typescript2';
import commonjs from 'rollup-plugin-commonjs';
import peerDepsExternal from 'rollup-plugin-peer-deps-external';
import pkg from './package.json' with { type: 'json' };
import path from "path";
import { existsSync, writeFileSync, mkdirSync } from 'fs';
import { join } from 'path';

/**
 * Rollup plugin that fails the build when the package manifest exports a path that was not produced.
 *
 * Nothing else catches this. `yarn ci` runs `tsc -b` and never rollup, so the bundle it would have
 * checked does not exist when the gate runs; the declarations resolve on their own, so TypeScript is
 * happy either way; and the only thing left to notice is a consumer deep-importing the subpath at
 * runtime. That is how `./json` and `./geospatial` came to be exported and unresolvable.
 */
function verifyExportsResolve(pkg, packageRoot) {
    const collectTargets = value =>
        typeof value === 'string'
            ? [value]
            : Object.values(value ?? {}).flatMap(collectTargets);

    return {
        name: 'verify-exports-resolve',
        closeBundle() {
            const missing = collectTargets(pkg.exports)
                .filter(target => !existsSync(join(packageRoot, target)));

            if (missing.length > 0) {
                this.error(`package.json "exports" points at ${missing.length} path(s) the build did not produce:\n  ${missing.join('\n  ')}`);
            }

            console.log(`✓ Verified every "exports" target resolves`);
        }
    };
}

/**
 * Rollup plugin to generate package.json files in output directories
 * This ensures proper module resolution for both CJS and ESM formats
 */
function generatePackageJson(cjsPath, esmPath) {
    return {
        name: 'generate-package-json',
        buildEnd() {
            // Create CJS package.json
            const cjsDir = cjsPath;
            mkdirSync(cjsDir, { recursive: true });
            writeFileSync(
                join(cjsDir, 'package.json'),
                JSON.stringify({ type: 'commonjs' }, null, 2),
                'utf-8'
            );

            // Create ESM package.json
            const esmDir = esmPath;
            mkdirSync(esmDir, { recursive: true });
            writeFileSync(
                join(esmDir, 'package.json'),
                JSON.stringify({ type: 'module' }, null, 2),
                'utf-8'
            );

            console.log('\u2713 Generated package.json files for CJS and ESM outputs');
        }
    };
}

function rollup(cjsPath, esmPath, tsconfigPath, pkg) {
    return {
        // Every subpath the package manifest exports is its own entry. `preserveModules` writes one
        // output file per module it keeps, but a barrel that only re-exports is not kept: rollup
        // rewrites importers to reach past it, so nothing references it and it is never emitted. The
        // manifest still pointed `./json` and `./geospatial` at those files, and TypeScript resolved
        // them through their declarations, so a consumer deep-importing either one compiled and then
        // failed at runtime with ERR_MODULE_NOT_FOUND.
        input: ["index.ts", "json/index.ts", "geospatial/index.ts", "reflection.ts"],

        output: [
            {
                dir: cjsPath,
                format: "cjs",
                exports: "named",
                sourcemap: true,
                preserveModules: true,
                preserveModulesRoot: "."
            },
            {
                dir: esmPath,
                format: "es",
                exports: "named",
                sourcemap: true,
                preserveModules: true,
                preserveModulesRoot: "."
            }
        ],
        external: [
            ...Object.keys(pkg.dependencies || {}),
            ...Object.keys(pkg.peerDependencies || {}),
            /^@cratis\/fundamentals/,
            'react',
            'react-dom',
        ],
        plugins: [
            peerDepsExternal(),
            commonjs({
                include: /node_modules/,
                esmExternals: true,
                namedExports: {
                    'react/jsx-runtime': ['tsx', 'jsx', 'jsxs'],
                },
            }),
            typescript2({
                exclude: "for_**/**/*",
                tsconfig: tsconfigPath,
                clean: true
            }),
            generatePackageJson(cjsPath, esmPath),
            verifyExportsResolve(pkg, import.meta.dirname)
        ]
    };
}

const cjsPath = path.dirname(pkg.main);
const esmPath = path.dirname(pkg.module);
const tsconfigPath = path.join(import.meta.dirname, "tsconfig.json");

export default rollup(cjsPath, esmPath, tsconfigPath, pkg);
