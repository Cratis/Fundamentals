// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
import { spawnSync } from 'node:child_process';
import { randomUUID } from 'node:crypto';
import { appendFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join, relative, resolve } from 'node:path';
import { assertSameHost, hostMetadata } from './host.mjs';

export const variants = ['V0', 'V4', 'V6'];

export function schedule(repetitions) {
    if (!Number.isInteger(repetitions) || repetitions < 1) throw new Error('Expected at least one repetition');
    const cases = variants.map(variant => ({ variant, run: 0 }));
    for (let run = 1; run <= repetitions; run++) {
        const offset = (run - 1) % variants.length;
        const order = [...variants.slice(offset), ...variants.slice(0, offset)];
        for (const variant of order) cases.push({ variant, run });
    }
    return cases;
}

function execute(command, cwd, env = process.env, dryRun = false) {
    console.log(`+ (${cwd}) ${command.join(' ')}`);
    if (dryRun) return;
    const result = spawnSync(command[0], command.slice(1), { cwd, env, stdio: 'inherit' });
    if (result.error) throw result.error;
    if (result.status !== 0) throw new Error(`Command exited ${result.status}: ${command.join(' ')}`);
}

function isolateAnalyzerConfig(source, dryRun) {
    const config = join(source, '.globalconfig');
    if (dryRun || !existsSync(config)) return;
    // Nested archives otherwise inherit the checkout's identical global config at equal precedence.
    appendFileSync(config, '\nglobal_level = 101\n');
}

export function prepareSources({ repo, output, baseline, patchRoot, dryRun = false }) {
    if (existsSync(output)) throw new Error(`Refusing to overwrite trial evidence: ${output}`);
    if (!dryRun) mkdirSync(output, { recursive: true });
    execute(['git', 'merge-base', '--is-ancestor', baseline, 'HEAD'], repo, process.env, dryRun);
    for (const variant of variants) {
        const source = join(output, variant, 'source');
        if (!dryRun) mkdirSync(source, { recursive: true });
        execute(['bash', '-c', 'set -euo pipefail; git archive "$1" | tar -x -C "$2"', '--', baseline, source], repo, process.env, dryRun);
        isolateAnalyzerConfig(source, dryRun);
        if (variant === 'V0') continue;
        const patch = join(patchRoot, `${variant}.patch`);
        execute(['git', 'apply', '--check', `--directory=${relative(repo, source)}`, patch], repo, process.env, dryRun);
        execute(['git', 'apply', `--directory=${relative(repo, source)}`, patch], repo, process.env, dryRun);
    }
}

function runVariant({ repo, output, session, phase, dryRun }, variant, run) {
    const env = {
        ...process.env, N: String(session.repetitions), TRIAL_OUTPUT: output, TRIAL_SESSION_ID: session.session_id,
        TRIAL_SOURCE_ROOT: join(output, variant, 'source')
    };
    delete env.TRIAL_RUN_INDEX;
    if (run !== undefined) env.TRIAL_RUN_INDEX = String(run);
    const command = [process.execPath, join(repo, '.trial/run.mjs'), variant, phase];
    if (dryRun) command.push('--dry-run');
    // Dry runs invoke only the print-only child driver; no source trees, builds or results are written.
    execute(command, repo, env);
}

export function main(args = process.argv.slice(2)) {
    const dryRun = args.includes('--dry-run');
    const phase = args.filter(argument => argument !== '--dry-run')[0];
    if (!['prepare', 'restore', 'build', 'test'].includes(phase)) throw new Error('Expected prepare|restore|build|test [--dry-run]');
    const repo = resolve('.');
    const output = resolve(process.env.TRIAL_OUTPUT ?? '.ai-work/runner-77-same-host');
    if (!relative(join(repo, '.ai-work'), output).match(/^(?!\.\.)(?!$)/)) throw new Error('Output must be under .ai-work');
    const baseline = JSON.parse(readFileSync('.trial/manifest.json', 'utf8')).baseline;
    const repetitions = Number(process.env.N ?? 5);
    const cases = schedule(repetitions);
    const sessionPath = join(output, 'session.json');
    const plannedSession = { ...hostMetadata(), session_id: randomUUID(), baseline, repetitions, cases };
    if (phase === 'prepare') {
        prepareSources({ repo, output, baseline, patchRoot: resolve('.trial'), dryRun });
        if (!dryRun) writeFileSync(sessionPath, `${JSON.stringify(plannedSession, null, 2)}\n`, { flag: 'wx' });
        return;
    }
    const session = dryRun ? plannedSession : JSON.parse(readFileSync(sessionPath, 'utf8'));
    if (session.baseline !== baseline) throw new Error('Session baseline mismatch');
    if (session.repetitions !== repetitions) throw new Error('Session repetition mismatch');
    assertSameHost([session, session, { ...hostMetadata(), session_id: session.session_id }]);
    const context = { repo, output, session, phase, dryRun };
    if (phase === 'test') {
        for (const { variant, run } of session.cases) runVariant(context, variant, run);
    } else {
        for (const variant of variants) runVariant(context, variant);
    }
}

if (process.argv[1] === new URL(import.meta.url).pathname) main();
