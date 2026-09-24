// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, readFileSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';

const packageRoot = resolve(import.meta.dirname, '..');
const manifest = JSON.parse(readFileSync(join(packageRoot, 'package.json'), 'utf8'));
const entries = Object.keys(manifest.exports ?? {});
if (!entries.includes('.') || entries.length < 2) throw new Error('Missing package entry points');

function run(command, args, cwd) {
    const result = spawnSync(command, args, { cwd, stdio: 'inherit' });
    if (result.error) throw result.error;
    if (result.status !== 0) throw new Error(`${command} ${args.join(' ')} failed (${result.status ?? result.signal})`);
}

const relativeImport = /\b(?:from\s*|import\s*\(|import\s*|require\s*\()(['"])(\.{1,2}\/[^'"\n]+)\1/g;
function checkImports(file, content) {
    let checked = 0;
    for (const [, , specifier] of content.matchAll(relativeImport)) {
        const target = resolve(dirname(file), specifier);
        const expected = file.endsWith('.d.ts') ? target.replace(/\.js$/, '.d.ts') : target;
        if (!specifier.endsWith('.js') || !existsSync(expected)) {
            throw new Error(`Unresolvable relative specifier ${specifier} in ${file}`);
        }
        checked++;
    }
    return checked;
}

const temporaryRoot = mkdtempSync(join(tmpdir(), 'fundamentals-consumer-'));
try {
    const planted = join(temporaryRoot, 'planted.d.ts');
    for (const invalid of ['./missing', './missing.js']) {
        let detected = false;
        try { checkImports(planted, `export * from '${invalid}';`); }
        catch { detected = true; }
        if (!detected) throw new Error(`Checker missed planted violation: ${invalid}`);
    }
    if (process.argv.includes('--self-test')) console.log('Detected 2 planted declaration violations');
    const packed = spawnSync('npm', ['pack', '--ignore-scripts', '--json', '--pack-destination', temporaryRoot], {
        cwd: packageRoot, encoding: 'utf8'
    });
    if (packed.error) throw packed.error;
    if (packed.status !== 0) throw new Error(`npm pack failed (${packed.status ?? packed.signal}): ${packed.stderr}`);
    const archive = join(temporaryRoot, Object.values(JSON.parse(packed.stdout))[0].filename);
    run('npm', ['install', '--ignore-scripts', '--no-audit', '--no-fund', '--no-package-lock', archive], temporaryRoot);

    const installed = join(temporaryRoot, 'node_modules', '@cratis', 'fundamentals');
    let checked = 0;
    for (const format of ['esm', 'cjs']) {
        const dist = join(installed, 'dist', format);
        for (const name of readdirSync(dist, { recursive: true }).filter(name => name.endsWith('.js') || name.endsWith('.d.ts'))) {
            const file = join(dist, name);
            checked += checkImports(file, readFileSync(file, 'utf8'));
        }
    }
    if (checked === 0) throw new Error('No relative imports checked');
    console.log(`Checked ${checked} packed JS and declaration imports`);

    const source = readFileSync(join(import.meta.dirname, 'fixtures', 'consumer.mts'), 'utf8');
    const compiler = join(packageRoot, '..', '..', 'node_modules', '@typescript', 'old', 'bin', 'tsc');
    for (const [mode, extension, module] of [
        ['NodeNext ESM', 'mts', 'NodeNext'],
        ['NodeNext CJS', 'cts', 'NodeNext'],
        ['Bundler', 'ts', 'ESNext']
    ]) {
        const directory = join(temporaryRoot, extension);
        mkdirSync(directory);
        writeFileSync(join(directory, `consumer.${extension}`), source);
        writeFileSync(join(directory, 'tsconfig.json'), JSON.stringify({
            compilerOptions: {
                module, moduleResolution: module === 'NodeNext' ? 'NodeNext' : 'Bundler',
                target: 'ES2022', strict: true, skipLibCheck: false, noEmit: true, types: []
            },
            files: [`consumer.${extension}`]
        }));
        run(process.execPath, [compiler, '--project', directory], temporaryRoot);
        console.log(`${mode} consumer type-check passed`);
    }

    const imports = entries.map(entry => entry === '.' ? manifest.name : `${manifest.name}/${entry.slice(2)}`);
    const smoke = `
        const entries = ${JSON.stringify(imports)};
        for (const entry of entries) {
            await import(entry, entry.endsWith('/package.json') ? { with: { type: 'json' } } : undefined);
        }
        const { Guid } = await import('${manifest.name}');
        const { Point } = await import('${manifest.name}/geospatial');
        if (!Guid.empty || new Point(1, 2).longitude !== 1) throw new Error('ESM entry points failed');
        console.log('Native ESM imports passed: ' + entries.length + ' entry points');
    `;
    run(process.execPath, ['--input-type=module', '-e', smoke], temporaryRoot);
    const requireSmoke = `
        for (const entry of ${JSON.stringify(imports)}) require(entry);
        if (!require('${manifest.name}').Guid.empty) throw new Error('CJS entry points failed');
        console.log('Native CJS requires passed: ${entries.length} entry points');
    `;
    run(process.execPath, ['-e', requireSmoke], temporaryRoot);
} finally {
    rmSync(temporaryRoot, { recursive: true, force: true });
}
