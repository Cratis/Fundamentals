// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
import { cpus, hostname, release } from 'node:os';

export function hostMetadata() {
    return {
        session_id: process.env.TRIAL_SESSION_ID ?? '', host_name: hostname(),
        runner_name: process.env.RUNNER_NAME ?? 'local', run_id: process.env.GITHUB_RUN_ID ?? 'local',
        run_attempt: process.env.GITHUB_RUN_ATTEMPT ?? 'local',
        platform: process.platform, os_release: release(),
        runner_image: process.env.ImageOS ?? 'local', runner_image_version: process.env.ImageVersion ?? 'local',
        cpu_count: cpus().length, cpu_model: cpus()[0]?.model
    };
}

export function assertSameHost(environments) {
    if (environments.length !== 3) throw new Error('Same-host validation requires all three variants');
    for (const field of Object.keys(hostMetadata())) {
        const values = environments.map(environment => environment[field]);
        if (values.some(value => !value)) throw new Error(`Missing same-host metadata: ${field}`);
        if (new Set(values).size !== 1) throw new Error(`Mixed-host measurements: ${field} differs`);
    }
}
