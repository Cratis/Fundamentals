// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/// <reference types="vitest/config" />
import { fileURLToPath } from 'node:url';
import commonjs from 'vite-plugin-commonjs';

export function createConfig() {
    return {
        optimizeDeps: {
            exclude: ['tslib'],
        },
        resolve: {
            tsconfigPaths: true,
        },
        test: {
            globals: true,
            environment: 'node',
            sourcemap: false,
            isolate: false,
            fileParallelism: false,
            pool: 'threads',
            mock: {
                exclude: ['**/node_modules/**', 'node_modules/**'],
            },
            coverage: {
                exclude: [
                    '**/for_*/**',
                    '**/wwwroot/**',
                    '**/api/**',
                    '**/Api/**',
                    '**/dist/**',
                    '**/*.test.tsx',
                    '**/*.d.ts',
                    '**/declarations.ts',
                ],
            },
            exclude: ['**/dist/**', '**/node_modules/**', 'node_modules/**', '**/wwwroot/**', 'wwwroot/**'],
            include: ['**/for_*/when_*/**/*.ts', '**/for_*/**/when_*.ts'],
            setupFiles: fileURLToPath(new URL('./vitest.setup.ts', import.meta.url))
        },
        plugins: [
            commonjs()
        ]
    };
}
