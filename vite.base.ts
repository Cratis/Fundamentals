/// <reference types="vitest/config" />
import commonjs from 'vite-plugin-commonjs';

const cwd = process.cwd();

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
            setupFiles: `${__dirname}/vitest.setup.ts`
        },
        plugins: [
            commonjs()
        ]
    };
}
