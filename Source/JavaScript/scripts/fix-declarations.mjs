// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { existsSync, readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';

const dist = resolve(import.meta.dirname, '../dist');
let rewritten = 0;
let declarations = 0;
const specifierPattern = /\b(from\s*|import\s*\(|import\s*)(['"])(\.{1,2}\/[^'"\n]+)\2/g;

for (const format of ['esm', 'cjs']) {
    const root = join(dist, format);
    for (const name of readdirSync(root, { recursive: true }).filter(name => name.endsWith('.d.ts'))) {
        const file = join(root, name);
        const original = readFileSync(file, 'utf8');
        const updated = original.replace(specifierPattern, (match, prefix, quote, specifier) => {
            const target = resolve(dirname(file), specifier);
            if (specifier.endsWith('.js')) {
                if (!existsSync(target.replace(/\.js$/, '.d.ts'))) throw new Error(`Missing declaration for ${specifier} in ${file}`);
                return match;
            }
            const resolved = existsSync(`${target}.d.ts`) ? `${specifier}.js`
                : existsSync(join(target, 'index.d.ts')) ? `${specifier}/index.js`
                : undefined;
            if (!resolved) throw new Error(`Unresolvable declaration import ${specifier} in ${file}`);
            rewritten++;
            return `${prefix}${quote}${resolved}${quote}`;
        });
        if (updated !== original) writeFileSync(file, updated);
        declarations++;
    }
}
if (declarations === 0 || rewritten === 0) throw new Error('No declaration imports to rewrite');
console.log(`Rewrote ${rewritten} imports in ${declarations} declarations`);
