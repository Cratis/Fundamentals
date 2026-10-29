// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { spawnSync } from 'node:child_process';
import { accessSync, constants, mkdirSync, readFileSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { parseArgs } from 'node:util';

function attributes(text) {
    const entities = { amp: '&', lt: '<', gt: '>', quot: '"', apos: "'" };
    return Object.fromEntries([...text.matchAll(/([\w:]+)="([^"]*)"/g)].map(([, key, value]) =>
        [key, value.replace(/&(amp|lt|gt|quot|apos);/g, (_, entity) => entities[entity])]));
}

function definitions(xml) {
    const tests = new Map();
    for (const [, attrs, body] of xml.matchAll(/<UnitTest\b([^>]*)>([\s\S]*?)<\/UnitTest>/g)) {
        const { id } = attributes(attrs);
        const methods = [...body.matchAll(/<TestMethod\b([^>]*?)\s*\/>/g)];
        if (!id) throw Error('UnitTest requires an id');
        if (tests.has(id)) throw Error(`Duplicate UnitTest id: ${id}`);
        if (methods.length !== 1) throw Error('Expected one TestMethod per UnitTest');
        const { className, name } = attributes(methods[0][1]);
        if (!className || !name) throw Error('TestMethod requires className and name');
        tests.set(id, `${className}.${name}`);
    }
    return tests;
}

function validateTotal(xml, count) {
    const counters = [...xml.matchAll(/<Counters\b([^>]*?)\s*\/>/g)];
    const total = counters.length === 1 ? attributes(counters[0][1]).total : '';
    if (!/^[1-9]\d*$/.test(total ?? '') || Number(total) !== count) {
        throw Error('Expected one positive Counters total matching the result count');
    }
}

function results(xml, tests) {
    const facts = new Map();
    const entries = [...xml.matchAll(/<UnitTestResult\b([^>]*?)(?:\/>|>[\s\S]*?<\/UnitTestResult>)/g)];
    validateTotal(xml, entries.length);
    for (const [, attrs] of entries) {
        const { testId, outcome } = attributes(attrs);
        const identity = tests.get(testId);
        if (!identity || !outcome) throw Error(`Result must join one UnitTest and have an outcome: ${testId}`);
        if (facts.has(identity)) throw Error(`Duplicate identity: ${identity}`);
        facts.set(identity, outcome);
    }
    return facts;
}

function readTrx(path) {
    try {
        const xml = readFileSync(path, 'utf8').replace(/^\uFEFF/, '').trim();
        if (!/^(?:<\?xml[^?]*\?>\s*)?<TestRun\b[^>]*>[\s\S]*<\/TestRun>$/.test(xml)) {
            throw Error('Expected a TestRun root');
        }
        return results(xml, definitions(xml));
    } catch (error) {
        throw Error(`Invalid TRX ${path}: ${error.message}`);
    }
}

function run(mode, executable, directory) {
    // Remove the previous report immediately before each run: a successful no-op must not reuse it.
    rmSync(join(directory, `${mode}.trx`), { force: true });
    const child = spawnSync(executable, ['--no-banner', '--no-ansi', '--no-progress', '--specs-cache', 'none',
        '--results-directory', directory, '--report-trx', '--report-trx-filename', `${mode}.trx`], { encoding: 'utf8' });
    const log = (child.stdout ?? '') + (child.stderr ?? '');
    writeFileSync(join(directory, `${mode}.log`), log);
    process.stdout.write(log);
    const status = child.status ?? 2;
    if (status !== 0) console.error(`::error::${mode} exited with ${status}${child.error ? `: ${child.error.message}` : ''}`);
    return status;
}

function failedFacts(mode, facts) {
    return [...facts].filter(([, outcome]) => outcome !== 'Passed')
        .map(([identity, outcome]) => `${mode}: ${identity}: ${outcome}`);
}

function compare(jit, native) {
    const missing = [...jit.keys()].filter(identity => !native.has(identity));
    const extra = [...native.keys()].filter(identity => !jit.has(identity));
    const totals = jit.size === native.size ? [] : [`Fact totals differ: JIT ${jit.size}, native ${native.size}`];
    const errors = [...totals, ...missing.map(identity => `Missing native identity: ${identity}`),
        ...extra.map(identity => `Extra native identity: ${identity}`),
        ...failedFacts('jit', jit), ...failedFacts('native', native)];
    if (errors.length) throw Error(errors.join('\n'));
}

function options() {
    const { values } = parseArgs({ options: { jit: { type: 'string' }, native: { type: 'string' },
        results: { type: 'string' }, label: { type: 'string' } } });
    for (const required of ['jit', 'native', 'results']) {
        if (!values[required]) throw Error(`Required option: --${required}`);
    }
    for (const mode of ['jit', 'native']) {
        accessSync(values[mode], constants.X_OK);
        if (!statSync(values[mode]).isFile()) throw Error(`Not an executable file: ${values[mode]}`);
    }
    values.results = resolve(values.results);
    mkdirSync(values.results, { recursive: true });
    return values;
}

function verify(values) {
    const facts = {};
    for (const mode of ['jit', 'native']) {
        const status = run(mode, resolve(values[mode]), values.results);
        if (status !== 0) return status;
        facts[mode] = readTrx(join(values.results, `${mode}.trx`));
    }
    compare(facts.jit, facts.native);
    console.log(`Native AOT.Specs matched JIT: ${facts.jit.size} facts, identities and outcomes equal, all passed${values.label ? ` (${values.label})` : ''}`);
    return 0;
}

function main() {
    let values;
    try {
        values = options();
    } catch (error) {
        console.error(`::error::${error.message}`);
        return 2;
    }
    try {
        return verify(values);
    } catch (error) {
        console.error(`::error::${error.message}`);
        return 1;
    }
}

process.exitCode = main();
