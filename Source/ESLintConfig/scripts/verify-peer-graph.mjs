// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import { mkdirSync, mkdtempSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const packageRoot = fileURLToPath(new URL('../', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../../', import.meta.url));
const artifacts = join(repoRoot, '.ai-work');
mkdirSync(artifacts, { recursive: true });
const workDir = mkdtempSync(join(artifacts, 'eslint-peer-'));
const npm = process.platform === 'win32' ? 'npm.cmd' : 'npm';
const run = (args, cwd = workDir) => execFileSync(npm, args, { cwd, stdio: 'inherit' });

console.log(`Verifying packed ESLint config in ${workDir}`);
const packed = JSON.parse(execFileSync(npm, ['pack', packageRoot, '--json', '--pack-destination', workDir], {
    cwd: workDir,
    encoding: 'utf8',
    stdio: ['ignore', 'pipe', 'inherit'],
}));
const tarball = join(workDir, packed[0].filename);
const consumerDir = join(workDir, 'consumer');
mkdirSync(consumerDir);
// Pin the registry's latest versions for this run. A bare "latest" request can be
// replaced by an older peer-compatible version during npm's dependency resolution.
const [eslintVersion, compilerVersion, apiVersion] = await Promise.all(
    ['eslint', 'typescript', '@typescript/typescript6'].map(async name => {
        const response = await fetch(`https://registry.npmjs.org/${encodeURIComponent(name)}/latest`);
        assert.ok(response.ok, `Could not resolve latest ${name}: HTTP ${response.status}`);
        const metadata = await response.json();
        assert.equal(typeof metadata.version, 'string');
        return metadata.version;
    }),
);
console.log(`Latest consumer: ESLint ${eslintVersion}, TypeScript compiler ${compilerVersion}, API compatibility ${apiVersion}`);
writeFileSync(join(consumerDir, 'package.json'), JSON.stringify({
    private: true,
    type: 'module',
    scripts: { check: 'node verify.mjs' },
    devDependencies: {
        '@cratis/eslint-config': `file:${tarball}`,
        eslint: eslintVersion,
        '@typescript/native': `npm:typescript@${compilerVersion}`,
        typescript: `npm:@typescript/typescript6@${apiVersion}`,
    },
}, null, 2));

writeFileSync(join(consumerDir, 'verify.mjs'), `
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { Linter } from 'eslint';
import cratis from '@cratis/eslint-config';

const metadata = JSON.parse(readFileSync('node_modules/@cratis/eslint-config/package.json', 'utf8'));
assert.equal(metadata.dependencies['eslint-plugin-react'], undefined);
const linter = new Linter();
const lint = (source, config) => linter.verify(source, config, { filename: 'sample.tsx' });
const rules = messages => messages.map(message => message.ruleId);
const valid = "import { Button } from './Button';\\nexport const View = () => <Button />;\\n";
assert.deepEqual(lint(valid, cratis.configs.consumer), []);
assert.ok(rules(lint("import { Unused } from './Button';\\nexport const View = () => <div />;\\n", cratis.configs.consumer)).includes('@typescript-eslint/no-unused-vars'));
assert.ok(rules(lint('export const value: any = 1;\\n', cratis.configs.consumer)).includes('@typescript-eslint/no-explicit-any'));
assert.ok(rules(lint('export const value = 1\\n', cratis.configs.consumer)).includes('semi'));
assert.ok(rules(lint('export const View = () => <div />;\\n', cratis.configs.internal)).includes('@tony.ganchev/header'));
const header = '// Copyright (c) Cratis. All rights reserved.\\n// Licensed under the MIT license. See LICENSE file in the project root for full license information.\\n\\n';
assert.deepEqual(lint(header + valid, cratis.configs.internal), []);
console.log('PASS: packed consumer/internal presets preserve JSX, unused-variable, semicolon, and header behavior');
`);

// A workspace install can hide published peer problems through hoisting and packageExtensions.
// The isolated tarball consumer has neither, and strict peer validation may not be bypassed.
run(['install', '--strict-peer-deps', '--no-audit', '--no-fund'], consumerDir);
run(['ls', '--all'], consumerDir);
const compilerOutput = execFileSync(npm, ['exec', '--no', '--', 'tsc', '--version'], { cwd: consumerDir, encoding: 'utf8' }).trim();
assert.equal(compilerOutput, `Version ${compilerVersion}`, 'The build must use the latest native compiler, not the compatibility executable');
console.log(compilerOutput);
run(['run', 'check'], consumerDir);
console.log('PASS: latest ESLint consumer installs and lints with a valid complete peer graph');
