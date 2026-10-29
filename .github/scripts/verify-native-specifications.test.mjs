// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { test } from 'node:test';

const scripts = dirname(fileURLToPath(import.meta.url));
const root = resolve(scripts, '../..');
const facts = ['should_one', 'should_two', 'should_three'];

function trx(names = facts, outcome = 'Passed') {
    return `<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results>${names.map((name, id) =>
        `<UnitTestResult testId="${id}" outcome="${outcome}" />`).join('')}</Results><TestDefinitions>${names.map((name, id) =>
        `<UnitTest id="${id}"><TestMethod className="Specs.when_running" name="${name}" /></UnitTest>`).join('')}</TestDefinitions><ResultSummary><Counters total="${names.length}" /></ResultSummary></TestRun>`;
}

function fixture(t, options = {}) {
    mkdirSync(join(root, '.ai-work'), { recursive: true });
    const directory = mkdtempSync(join(root, '.ai-work/verify-native-specifications-test-'));
    t.after(() => rmSync(directory, { recursive: true }));
    const results = join(directory, 'results');
    mkdirSync(results);
    for (const mode of ['jit', 'native']) {
        if (options.stale) writeFileSync(join(results, `${mode}.trx`), trx());
        if (options.missing === mode) continue;
        const document = options[mode] ?? trx();
        writeFileSync(join(directory, mode), `#!/usr/bin/env bash
set -eu
[[ "$1" == --no-banner && "$2" == --no-ansi && "$3" == --no-progress ]]
[[ "$4" == --specs-cache && "$5" == none && "$6" == --results-directory ]]
[[ "$8" == --report-trx && "$9" == --report-trx-filename && "\${10}" == ${mode}.trx ]]
echo '${mode} executed'
${document === false ? ':' : `printf '%s' '${document.replaceAll("'", "'\\''")}' > "$7/\${10}"`}
exit ${options[`${mode}Exit`] ?? 0}
`, { mode: options.notExecutable === mode ? 0o644 : 0o755 });
    }
    const result = spawnSync(process.execPath, [join(scripts, 'verify-native-specifications.mjs'),
        '--jit', join(directory, 'jit'), '--native', join(directory, 'native'), '--results', results, '--label', 'test-rid'],
    { encoding: 'utf8', timeout: 10000 });
    assert.ifError(result.error);
    return { ...result, output: result.stdout + result.stderr };
}

test('matches three fact identities and outcomes', t => {
    const result = fixture(t);
    assert.equal(result.status, 0, result.output);
    assert.match(result.output, /Native AOT.Specs matched JIT: 3 facts, identities and outcomes equal, all passed \(test-rid\)/);
});

for (const mode of ['jit', 'native']) {
    test(`checks missing ${mode} before running either executable`, t => {
        const result = fixture(t, { missing: mode });
        assert.equal(result.status, 2, result.output);
        assert.doesNotMatch(result.output, /executed/);
    });
    test(`rejects non-executable ${mode}`, t => {
        assert.equal(fixture(t, { notExecutable: mode }).status, 2);
    });
    test(`propagates ${mode} exit`, t => {
        const status = mode === 'jit' ? 8 : 2;
        const result = fixture(t, { [`${mode}Exit`]: status });
        assert.equal(result.status, status, result.output);
        assert.match(result.output, new RegExp(`::error::${mode} exited with ${status}`));
        if (mode === 'jit') assert.doesNotMatch(result.output, /native executed/);
    });
    for (const [name, document] of [
        ['missing', false], ['empty', ''], ['invalid root', '<Other />'], ['zero results', trx([])],
        ['wrong total', trx().replace('total="3"', 'total="4"')],
        ['missing counters', trx().replace(/<Counters[^>]*\/>/, '')],
        ['duplicate counters', trx().replace('<Counters', '<Counters total="3" /><Counters')],
        ['unjoined result', trx().replace('testId="0"', 'testId="unknown"')]
    ]) {
        test(`rejects ${mode} ${name} report`, t => {
            const result = fixture(t, { [mode]: document });
            assert.equal(result.status, 1, result.output);
            assert.match(result.output, /Invalid TRX/);
        });
    }
    test(`rejects stale ${mode} report when process writes nothing`, t => {
        const result = fixture(t, { stale: true, [mode]: false });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /Invalid TRX/);
    });
    test(`rejects failed ${mode} facts`, t => {
        const result = fixture(t, { [mode]: trx(facts, 'Failed') });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /Specs.when_running.should_one.*Failed/);
    });
}

for (const [name, names, identity] of [
    ['missing', facts.slice(1), 'should_one'],
    ['extra', [...facts, 'should_extra'], 'should_extra'],
    ['duplicate', [...facts, facts[0]], 'should_one']
]) {
    test(`rejects ${name} native identity`, t => {
        const result = fixture(t, { native: trx(names) });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, new RegExp(`Specs.when_running.${identity}`));
    });
}

test('rejects unequal JIT and native totals even when both reports are internally consistent', t => {
    const result = fixture(t, { native: trx(facts.slice(1)) });
    assert.equal(result.status, 1, result.output);
    assert.match(result.output, /Fact totals differ: JIT 3, native 2/);
});

test('decodes all five XML entities in identities', t => {
    const result = fixture(t, { jit: trx(['should_&lt;&gt;&quot;&apos;&amp;']), native: trx(['should_&lt;&gt;&quot;&apos;&amp;']) });
    assert.equal(result.status, 0, result.output);
});
