// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
import { spawnSync } from 'node:child_process';
import { appendFileSync, readFileSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';

function invalidValue(value) {
    return !Number.isFinite(value) || value < 0;
}

export function median(values) {
    if (values.length === 0) throw new Error('Missing measurements');
    if (values.some(invalidValue)) throw new Error('Invalid measurements');
    const sorted = [...values].sort((left, right) => left - right);
    return (sorted[Math.floor((sorted.length - 1) / 2)] + sorted[Math.floor(sorted.length / 2)]) / 2;
}

function readJson(path) {
    return JSON.parse(readFileSync(path, 'utf8'));
}

function compareRun(baseline, directory, variant) {
    const parity = spawnSync('python3', ['-B', '.trial/parity.py', baseline, join(directory, 'results.trx'),
        '--variant', variant, '--manifest', '.trial/manifest.json'], { encoding: 'utf8' });
    console.log(`${directory}: ${parity.stdout}`);
    if (parity.error) throw parity.error;
    if (parity.status !== 0) throw new Error(parity.stdout + parity.stderr);
    const count = parity.stdout.match(/PARITY PASS: (\d+) unique case identities and outcomes/);
    if (!count) throw new Error('Missing parity count');
    if (Number(count[1]) === 0) throw new Error('Empty suite');
    return Number(count[1]);
}

function validateEnvironment(environment, variant, project, repetitions) {
    if (environment.variant !== variant) throw new Error('Unexpected variant');
    if (environment.repetitions !== repetitions) throw new Error('Unexpected repetition count');
    if (!environment.projects.includes(project)) throw new Error('Missing project');
    const manifest = readJson('.trial/manifest.json');
    if (environment.baseline !== manifest.baseline) throw new Error('Unexpected baseline');
}

function projectRow(root, variant, project, repetitions) {
    const resultRoot = join(root, variant, 'results');
    validateEnvironment(readJson(join(resultRoot, 'environment.json')), variant, project, repetitions);
    const baseline = join(root, 'V0', 'results', project, 'run-1', 'results.trx');
    compareRun(baseline, join(resultRoot, project, 'warmup'), variant);
    const timings = [];
    let tests;
    for (let run = 1; run <= repetitions; run++) {
        const directory = join(resultRoot, project, `run-${run}`);
        tests = compareRun(baseline, directory, variant);
        const timing = readJson(join(directory, 'timing.json'));
        if (timing.exit !== 0) throw new Error('Measured command failed');
        timings.push(timing);
    }
    const med = key => median(timings.map(timing => timing[key]));
    return `| ${project} | ${variant} | ${med('wall_seconds').toFixed(3)} | ${med('cpu_seconds').toFixed(3)} | ${(med('max_rss_bytes') / 1048576).toFixed(1)} | ${tests} | PASS |`;
}

function runnerLine(root, variant) {
    const environment = readJson(join(root, variant, 'results', 'environment.json'));
    return `- ${variant}: ${environment.runner_image} ${environment.runner_image_version}; ${environment.cpu_count} CPUs, ${environment.cpu_model}; baseline ${environment.baseline}`;
}

function safeProjectRow(root, variant, project) {
    try {
        return { line: projectRow(root, variant, project, 5), failed: false };
    } catch (error) {
        console.error(`${project} ${variant}: ${error.message}`);
        return { line: `| ${project} | ${variant} | — | — | — | — | FAIL / incomplete |`, failed: true };
    }
}

function safeRunnerLine(root, variant) {
    try {
        return { line: runnerLine(root, variant), failed: false };
    } catch {
        return { line: `- ${variant}: missing environment metadata`, failed: true };
    }
}

export function summarize(root) {
    const lines = [
        '# Specifications #77 runner trial', '',
        'One untimed warmup, then five measured runs per project. CPU = user + system; RSS is peak per run.',
        'Parity checks every warmup and measured run against V0 run 1 (identities and outcomes, not counts).', '',
        '| Project | Variant | Median wall (s) | Median CPU (s) | Median peak RSS (MiB) | Tests | Parity |',
        '| --- | --- | ---: | ---: | ---: | ---: | --- |'
    ];
    let failed = false;
    for (const project of ['Fundamentals.Specs', 'OpenTelemetry.Specs']) {
        for (const variant of ['V0', 'V4', 'V6']) {
            const result = safeProjectRow(root, variant, project);
            lines.push(result.line);
            failed = result.failed || failed;
        }
    }
    lines.push('', '## Runner environments', '');
    for (const variant of ['V0', 'V4', 'V6']) {
        const result = safeRunnerLine(root, variant);
        lines.push(result.line);
        failed = result.failed || failed;
    }
    return { markdown: `${lines.join('\n')}\n`, failed };
}

if (process.argv[1] === new URL(import.meta.url).pathname) {
    const root = resolve(process.argv[2] ?? '.ai-work/runner-77');
    const result = summarize(root);
    writeFileSync(join(root, 'summary.md'), result.markdown);
    if (process.env.GITHUB_STEP_SUMMARY) appendFileSync(process.env.GITHUB_STEP_SUMMARY, result.markdown);
    console.log(result.markdown);
    process.exitCode = result.failed ? 1 : 0;
}
