// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { copyFileSync, mkdirSync, mkdtempSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { test } from 'node:test';

const scripts = dirname(fileURLToPath(import.meta.url));
const root = resolve(scripts, '../..');

function pathWithoutPython(directory) {
    const tools = join(directory, 'tools');
    mkdirSync(tools);
    for (const [name, executable] of [['bash', '/bin/bash'], ['dirname', '/usr/bin/dirname']]) {
        writeFileSync(join(tools, name), `#!/bin/sh\nexec ${executable} "$@"\n`, { mode: 0o755 });
    }
    return tools;
}

function fixture(t, options = {}) {
    const { count = 5, missing = -1, exits = {}, totals = {}, documents = {}, noTrx = -1, windows = false, missingPython = false } = options;
    mkdirSync(join(root, '.ai-work'), { recursive: true });
    const directory = mkdtempSync(join(root, '.ai-work/run-specifications-test-'));
    t.after(() => rmSync(directory, { recursive: true }));
    const target = join(directory, '.github/scripts');
    mkdirSync(target, { recursive: true });
    copyFileSync(join(scripts, 'run-specifications.sh'), join(target, 'run-specifications.sh'));
    for (let index = 0; index < count; index++) {
        const project = join(directory, `Source/DotNET/Project${index}.Specs`);
        const output = join(project, 'bin/Release/net10.0');
        mkdirSync(output, { recursive: true });
        writeFileSync(join(project, `Project${index}.Specs.csproj`), `<Project><AssemblyName>Assembly${index}</AssemblyName></Project>`);
        if (index === missing) continue;
        const document = documents[index] ?? `<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><ResultSummary><Counters total="${totals[index] ?? 1}" /></ResultSummary></TestRun>`;
        writeFileSync(join(output, `Assembly${index}${windows ? '.exe' : ''}`), `#!/usr/bin/env bash
set -eu
[[ "$1" == -noLogo && "$2" == -noColor && "$3" == -result-trx ]]
echo 'Project${index} executed'
${index === noTrx ? ':' : `printf '%s' '${document.replaceAll("'", "'\\''")}' > "$4"`}
exit ${exits[index] ?? 0}
`, { mode: 0o755 });
    }
    const result = spawnSync('bash', [join(target, 'run-specifications.sh'), 'Release'], {
        encoding: 'utf8',
        timeout: 10000,
        env: {
            ...process.env,
            EXPECTED_SPECIFICATION_PROJECTS: '5',
            OS: windows ? 'Windows_NT' : 'Linux',
            PATH: missingPython ? pathWithoutPython(directory) : process.env.PATH
        }
    });
    assert.ifError(result.error);
    return { ...result, directory };
}

test('runs all five executables and reports their logs and totals', t => {
    const result = fixture(t);
    assert.equal(result.status, 0, result.stdout + result.stderr);
    assert.match(result.stdout, /Running 5 specification projects/);
    assert.equal((result.stdout.match(/executed/g) ?? []).length, 5);
    const runs = join(result.directory, 'TestResults');
    const files = readdirSync(join(runs, readdirSync(runs)[0]));
    assert.equal(files.filter(name => name.endsWith('.trx')).length, 5);
    assert.equal(files.filter(name => name.endsWith('.log')).length, 5);
});

test('requires Python before starting any project', t => {
    const result = fixture(t, { missingPython: true });
    assert.equal(result.status, 2);
    assert.match(result.stdout, /::error::python3 is required to validate specification TRX results/);
    assert.doesNotMatch(result.stdout, /executed/);
});

test('rejects an empty or incomplete project population', async t => {
    for (const count of [0, 4]) {
        await t.test(`${count} projects`, child => {
            const result = fixture(child, { count });
            assert.equal(result.status, 2);
            assert.match(result.stdout, new RegExp(`Expected 5 specification projects, found ${count}`));
        });
    }
});

test('checks every executable before starting any project', t => {
    const result = fixture(t, { missing: 4 });
    assert.equal(result.status, 2);
    assert.match(result.stdout, /Missing specification executable: .*Assembly4/);
    assert.doesNotMatch(result.stdout, /executed/);
});

test('waits for every project and preserves the first nonzero child status', t => {
    const result = fixture(t, { exits: { 0: 7, 1: 3 } });
    assert.equal(result.status, 7);
    assert.match(result.stdout, /::error::Project0.Specs exited with 7/);
    assert.match(result.stdout, /::error::Project1.Specs exited with 3/);
    assert.equal((result.stdout.match(/executed/g) ?? []).length, 5);
});

test('rejects zero tests even when the executable succeeds', t => {
    const result = fixture(t, { totals: { 2: 0 } });
    assert.equal(result.status, 1);
    assert.match(result.stdout, /::error::Project2.Specs produced no nonzero TRX test total/);
});

test('rejects a missing TRX instead of accepting stale output', t => {
    const result = fixture(t, { noTrx: 3 });
    assert.equal(result.status, 1);
    assert.match(result.stdout, /::error::Project3.Specs produced no nonzero TRX test total/);
});

test('accepts valid multiline TRX counters', t => {
    const result = fixture(t, { documents: { 2: `<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary>
    <Counters
      total="1"
      passed="1" />
  </ResultSummary>
</TestRun>` } });
    assert.equal(result.status, 0, result.stdout + result.stderr);
});

test('rejects invalid TRX even when the executable succeeds', async t => {
    const documents = {
        truncated: '<TestRun><ResultSummary><Counters total="1" />',
        malformed: '<TestRun><ResultSummary><Counters total="1" /></WrongSummary></TestRun>',
        empty: '',
        'empty counters': '<TestRun><ResultSummary><Counters /></ResultSummary></TestRun>',
        'missing counters': '<TestRun><ResultSummary /></TestRun>',
        'multiple counters': '<TestRun><ResultSummary><Counters total="1" /><Counters total="1" /></ResultSummary></TestRun>',
        'invalid total': '<TestRun><ResultSummary><Counters total="invalid" /></ResultSummary></TestRun>'
    };
    for (const [name, document] of Object.entries(documents)) {
        await t.test(name, child => {
            const result = fixture(child, { documents: { 2: document } });
            assert.equal(result.status, 1);
            assert.match(result.stdout, /::error::Project2.Specs produced no nonzero TRX test total/);
            assert.match(result.stderr, /Invalid TRX:/);
        });
    }
});

test('uses Windows apphost suffixes', t => {
    const result = fixture(t, { windows: true });
    assert.equal(result.status, 0, result.stdout + result.stderr);
});
