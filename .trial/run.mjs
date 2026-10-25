// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { measure } from './timing.mjs';
import { hostMetadata } from './host.mjs';

const dryRun = process.argv.includes('--dry-run');
const [variant, phase = 'all', selectedProject] = process.argv.slice(2).filter(argument => argument !== '--dry-run');
const projects = selectedProject ? [selectedProject] : ['Fundamentals.Specs', 'OpenTelemetry.Specs'];
const repetitions = Number(process.env.N ?? 5);
const selectedRun = process.env.TRIAL_RUN_INDEX;
const source = resolve(process.env.TRIAL_SOURCE_ROOT ?? '.');
if (!['V0', 'V4', 'V6'].includes(variant) || !['restore', 'build', 'test', 'all'].includes(phase)
    || projects.some(project => !['Fundamentals.Specs', 'OpenTelemetry.Specs'].includes(project))
    || !Number.isInteger(repetitions) || repetitions < 1) {
    throw new Error('Usage: node .trial/run.mjs V0|V4|V6 [restore|build|test|all] [Fundamentals.Specs|OpenTelemetry.Specs]; N >= 1');
}
if (selectedRun !== undefined && !/^(0|[1-9]\d*)$/.test(selectedRun)) throw new Error('Invalid run index');
if (Number(selectedRun) > repetitions) throw new Error('Run index exceeds repetitions');
const output = resolve(process.env.TRIAL_OUTPUT ?? '.ai-work/runner-77');
const artifacts = join(output, variant, 'build');
const results = join(output, variant, 'results');
const manifest = JSON.parse(readFileSync('.trial/manifest.json', 'utf8'));
function execute(command) {
    console.log(`+ ${command.join(' ')}`);
    if (dryRun) return;
    const result = spawnSync(command[0], command.slice(1), { stdio: 'inherit', cwd: source });
    if (result.error || result.status !== 0) {
        throw result.error ?? new Error(`Command failed: ${result.status} (${result.signal ?? 'no signal'})`);
    }
}

function projectPath(project) {
    return join(source, `Source/DotNET/${project}/${project}.csproj`);
}

function executable(project) {
    const definition = dryRun ? resolve(`Source/DotNET/${project}/${project}.csproj`) : projectPath(project);
    const assembly = readFileSync(definition, 'utf8').match(/<AssemblyName>([^<]+)<\/AssemblyName>/)?.[1];
    if (!assembly) throw new Error(`Missing AssemblyName: ${project}`);
    const folder = join(artifacts, 'bin', project);
    if (dryRun) return join(folder, 'release_net10.0', `${assembly}.dll`);
    const matches = readdirSync(folder).filter(pivot => pivot.startsWith('release') && pivot.includes('net10.0'))
        .map(pivot => join(folder, pivot, `${assembly}.dll`)).filter(existsSync);
    if (matches.length !== 1) throw new Error(`Expected one net10.0 executable: ${project}, found ${matches}`);
    return matches[0];
}

function testCommand(project, directory) {
    if (variant === 'V0') {
        return ['dotnet', 'test', projectPath(project), '--configuration', 'Release', '--framework', 'net10.0',
            '--no-build', '--no-restore', '--artifacts-path', artifacts, '--logger', 'trx;LogFileName=results.trx',
            '--results-directory', directory, '--blame-hang', '--blame-hang-timeout', '3m', '--blame-hang-dump-type', 'none'];
    }
    const command = ['dotnet', executable(project), '--report-trx', '--report-trx-filename', 'results.trx', '--results-directory', directory];
    if (variant === 'V6' && project === 'OpenTelemetry.Specs') command.push('--specs-parallel', '1', '--specs-sequential-facts');
    return command;
}

function timedCommand(command, directory) {
    const flags = process.platform === 'darwin' ? ['-l'] : ['-v', '-o', join(directory, 'timing.time.txt')];
    return ['/usr/bin/time', ...flags, ...command];
}

function runCase(project, run) {
    const directory = join(results, project, run === 0 ? 'warmup' : `run-${run}`);
    const command = testCommand(project, directory);
    console.log(`${variant} ${project} ${run === 0 ? 'untimed warmup' : `measured run ${run}/${repetitions}`}`);
    if (dryRun) {
        execute(run === 0 ? command : timedCommand(command, directory));
        return;
    }
    if (existsSync(directory)) throw new Error(`Refusing to overwrite results: ${directory}`);
    mkdirSync(directory, { recursive: true });
    if (run === 0) execute(command);
    else measure(command, join(directory, 'timing'), source);
    if (!existsSync(join(directory, 'results.trx'))) throw new Error(`Missing TRX: ${directory}`);
}

function runProject(project) {
    if (selectedRun !== undefined) {
        runCase(project, Number(selectedRun));
        return;
    }
    for (let run = 0; run <= repetitions; run++) runCase(project, run);
}

// Require the pinned baseline in the checkout history, never a moving remote reference.
execute(['git', 'merge-base', '--is-ancestor', manifest.baseline, 'HEAD']);
if (!dryRun) mkdirSync(results, { recursive: true });
const environment = {
    ...hostMetadata(), variant, baseline: manifest.baseline, source_root: source, repetitions, projects
};
console.log(JSON.stringify(environment, null, 2));
if (!dryRun) writeFileSync(join(results, 'environment.json'), `${JSON.stringify(environment, null, 2)}\n`);
function restoreProject(project) {
    execute(['dotnet', 'restore', projectPath(project), '--artifacts-path', artifacts, '-p:Configuration=Release']);
}

function buildProject(project) {
    execute(['dotnet', 'build', projectPath(project), '--configuration', 'Release', '--no-restore', '--artifacts-path', artifacts,
        '--disable-build-servers', '-p:UseSharedCompilation=false', '-nodeReuse:false', '-warnaserror']);
}

for (const project of projects) {
    if (phase === 'restore' || phase === 'all') restoreProject(project);
    if (phase === 'build' || phase === 'all') buildProject(project);
    if (phase === 'test' || phase === 'all') runProject(project);
}
