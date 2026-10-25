// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import test from 'node:test';
import { assertSameHost, hostMetadata } from './host.mjs';
import { prepareSources, schedule, variants } from './same-host.mjs';
import { summarize } from './summarize.mjs';

const base = resolve(process.env.AI_WORK_OUTPUT ?? '.ai-work/runner-77-harness');
mkdirSync(base, { recursive: true });
const scratch = () => mkdtempSync(join(base, 'same-host-'));
const baseline = JSON.parse(readFileSync('.trial/manifest.json', 'utf8')).baseline;
const projects = ['Fundamentals.Specs', 'OpenTelemetry.Specs'];
const environment = () => ({ ...hostMetadata(), session_id: 'self-test-session', baseline, repetitions: 5, projects });

function git(repo, args) {
    const result = spawnSync('git', args, { cwd: repo, encoding: 'utf8' });
    assert.equal(result.status, 0, result.stderr);
    return result.stdout.trim();
}

function trx(method = 'should_work', outcome = 'Passed') {
    return `<TestRun><TestDefinitions><UnitTest id="1"><TestMethod className="Example.when_ready" name="${method}" /></UnitTest></TestDefinitions><Results><UnitTestResult testId="1" testName="${method}" outcome="${outcome}" /></Results><ResultSummary><Counters total="1" /></ResultSummary></TestRun>`;
}

function writeRun(directory, contents, run) {
    mkdirSync(directory, { recursive: true });
    writeFileSync(join(directory, 'results.trx'), contents);
    if (run > 0) writeFileSync(join(directory, 'timing.json'), JSON.stringify({
        exit: 0, wall_seconds: run, cpu_seconds: run * 2, max_rss_bytes: 1048576 * run
    }));
}

function projectFixture(resultRoot, project, variant, options) {
    for (let run = 0; run <= 5; run++) {
        const finalCandidate = variant === 'V6' && run === 5;
        if (options.omitLastRun && finalCandidate) continue;
        const directory = join(resultRoot, project, run === 0 ? 'warmup' : `run-${run}`);
        writeRun(directory, finalCandidate ? options.changedCase : trx(), run);
    }
}

function fixture({ changedCase = trx(), mixedHost = false, omitLastRun = false } = {}) {
    const root = scratch();
    for (const variant of variants) {
        const resultRoot = join(root, variant, 'results');
        mkdirSync(resultRoot, { recursive: true });
        const metadata = { ...environment(), variant };
        if (mixedHost && variant === 'V6') metadata.cpu_model = 'another CPU';
        writeFileSync(join(resultRoot, 'environment.json'), JSON.stringify(metadata));
        for (const project of projects) projectFixture(resultRoot, project, variant, { changedCase, omitLastRun });
    }
    return root;
}

test('one warmup each; five synchronous interleaved rounds with rotating variant order', () => {
    const cases = schedule(5);
    assert.equal(cases.length, 18);
    assert.deepEqual(cases.slice(0, 3), variants.map(variant => ({ variant, run: 0 })));
    for (let run = 1; run <= 5; run++) {
        const round = cases.slice(run * 3, run * 3 + 3);
        assert.deepEqual(round.map(entry => entry.run), [run, run, run]);
        assert.deepEqual([...round.map(entry => entry.variant)].sort(), variants);
        assert.equal(round[0].variant, variants[(run - 1) % 3]);
    }
    assert.throws(() => schedule(0), /at least one/);
});

test('archives the pinned baseline, isolates patches, refuses overwrites and leaves checkout untouched', () => {
    const repo = scratch();
    git(repo, ['init', '--quiet']);
    writeFileSync(join(repo, 'value.txt'), 'baseline\n');
    writeFileSync(join(repo, '.globalconfig'), 'is_global = true\n');
    git(repo, ['add', 'value.txt', '.globalconfig']);
    git(repo, ['-c', 'user.name=Harness', '-c', 'user.email=harness@example.invalid', 'commit', '--quiet', '-m', 'Baseline fixture']);
    const revision = git(repo, ['rev-parse', 'HEAD']);
    writeFileSync(join(repo, 'value.txt'), 'uncommitted checkout\n');
    const patches = join(repo, 'patches');
    mkdirSync(patches);
    for (const variant of ['V4', 'V6']) writeFileSync(join(patches, `${variant}.patch`),
        `diff --git a/value.txt b/value.txt\n--- a/value.txt\n+++ b/value.txt\n@@ -1 +1 @@\n-baseline\n+${variant}\n`);
    const output = join(repo, '.ai-work', 'trial');
    const options = { repo, output, baseline: revision, patchRoot: patches };
    prepareSources(options);
    assert.equal(readFileSync(join(output, 'V0/source/value.txt'), 'utf8'), 'baseline\n');
    for (const variant of ['V4', 'V6']) assert.equal(readFileSync(join(output, variant, 'source/value.txt'), 'utf8'), `${variant}\n`);
    assert.equal(readFileSync(join(repo, 'value.txt'), 'utf8'), 'uncommitted checkout\n');
    assert.equal(existsSync(join(output, 'V0/source/.git')), false);
    assert.equal(readFileSync(join(repo, '.globalconfig'), 'utf8'), 'is_global = true\n');
    for (const variant of variants) assert.match(readFileSync(join(output, variant, 'source/.globalconfig'), 'utf8'), /global_level = 101/);
    assert.throws(() => prepareSources(options), /Refusing to overwrite/);
});

test('same-host validation rejects different CPUs, hostnames, sessions and missing metadata', () => {
    const original = environment();
    assert.doesNotThrow(() => assertSameHost([original, original, original]));
    for (const field of ['cpu_model', 'host_name', 'session_id']) {
        assert.throws(() => assertSameHost([original, original, { ...original, [field]: 'different' }]), /Mixed-host/);
    }
    assert.throws(() => assertSameHost([original, original, { ...original, host_name: '' }]), /Missing/);
    assert.throws(() => assertSameHost([original]), /all three/);
});

test('summary validates all 36 TRXs and reports correct medians', () => {
    const result = summarize(fixture());
    assert.equal(result.failed, false);
    assert.equal(result.markdown.match(/\| 3\.000 \| 6\.000 \| 3\.0 \| 1 \| PASS \|/g)?.length, 6);
});

test('summary refuses mixed-host evidence rather than publishing comparable timings', () => {
    const result = summarize(fixture({ mixedHost: true }));
    assert.equal(result.failed, true);
    assert.equal(result.markdown.includes('| PASS |'), false);
});

test('strict parity rejects changed identity with identical test counts', () => {
    assert.equal(summarize(fixture({ changedCase: trx('should_be_different') })).failed, true);
});

test('strict parity rejects changed outcomes with identical test counts', () => {
    assert.equal(summarize(fixture({ changedCase: trx('should_work', 'Failed') })).failed, true);
});

test('incomplete repetitions cannot produce a passing summary', () => {
    assert.equal(summarize(fixture({ omitLastRun: true })).failed, true);
});
