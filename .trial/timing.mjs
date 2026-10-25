// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
import { spawnSync } from 'node:child_process';
import { closeSync, openSync, readFileSync, writeFileSync } from 'node:fs';

function elapsedSeconds(value) {
    return value.split(':').reduce((seconds, part) => seconds * 60 + Number(part), 0);
}

function requiredMatch(text, pattern) {
    const match = text.match(pattern);
    if (!match) throw new Error(`Incomplete /usr/bin/time output: ${pattern}`);
    return match;
}

function linuxValues(text) {
    return {
        wall: requiredMatch(text, /Elapsed \(wall clock\) time \(h:mm:ss or m:ss\):\s*(\S+)/)[1],
        user: requiredMatch(text, /User time \(seconds\):\s*([\d.]+)/)[1],
        system: requiredMatch(text, /System time \(seconds\):\s*([\d.]+)/)[1],
        rss: requiredMatch(text, /Maximum resident set size \(kbytes\):\s*(\d+)/)[1]
    };
}

function darwinValues(text) {
    const [, wall, user, system] = requiredMatch(text, /([\d.]+) real\s+([\d.]+) user\s+([\d.]+) sys/);
    return { wall, user, system, rss: requiredMatch(text, /(\d+)\s+maximum resident set size/)[1] };
}

function invalidValue(value) {
    return !Number.isFinite(value) || value < 0;
}

export function parseTiming(text, platform) {
    const { wall, user, system, rss } = platform === 'darwin' ? darwinValues(text) : linuxValues(text);
    const timing = {
        wall_seconds: elapsedSeconds(wall), user_seconds: Number(user), system_seconds: Number(system),
        cpu_seconds: Number(user) + Number(system), max_rss_bytes: Number(rss) * (platform === 'darwin' ? 1 : 1024)
    };
    if (Object.values(timing).some(invalidValue)) throw new Error('Invalid /usr/bin/time values');
    if (timing.max_rss_bytes === 0) throw new Error('Missing maximum RSS');
    return timing;
}

export function measure(command, prefix) {
    const raw = `${prefix}.time.txt`;
    const log = openSync(`${prefix}.stdout.log`, 'wx');
    const error = process.platform === 'darwin' ? openSync(raw, 'wx') : log;
    const args = process.platform === 'darwin' ? ['-l'] : ['-v', '-o', raw];
    let result;
    try {
        result = spawnSync('/usr/bin/time', [...args, ...command], {
            stdio: ['ignore', log, error], env: { ...process.env, LC_ALL: 'C' }, timeout: 600_000
        });
    } finally {
        closeSync(log);
        if (error !== log) closeSync(error);
    }
    process.stdout.write(readFileSync(`${prefix}.stdout.log`));
    process.stderr.write(readFileSync(raw));
    if (result.error || result.status !== 0) {
        throw result.error ?? new Error(`Test command exited ${result.status} (${result.signal ?? 'no signal'})`);
    }
    const timing = { command, exit: result.status, ...parseTiming(readFileSync(raw, 'utf8'), process.platform) };
    writeFileSync(`${prefix}.json`, `${JSON.stringify(timing, null, 2)}\n`, { flag: 'wx' });
    return timing;
}
