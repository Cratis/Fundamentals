import tseslintPlugin from '@typescript-eslint/eslint-plugin';
import tsParser from '@typescript-eslint/parser';
import noNull from 'eslint-plugin-no-null';
import react from 'eslint-plugin-react';
import globals from 'globals';
import { coreRules } from './lib/coreRules.js';

// Default ignore globs shared across Cratis projects: build output, generated proxy
// `Api/` folders, scaffolding templates, declaration files, and plain JS tooling.
// A consumer can append its own ignores in a later config object.
export const ignores = [
    '**/*.d.ts',
    '**/*.scss.d.ts',
    '**/tsconfig.*',
    '**/wallaby.js',
    '**/*.js',
    '**/dist/**',
    '**/node_modules/**',
    '**/wwwroot/**',
    '**/templates/**',
    '**/Api/**',
    '**/rollup.config.mjs',
];

// Product-agnostic Cratis hygiene for TypeScript/React. No license-header rule here —
// see `internal` for the Cratis-authoring layer.
const base = [
    { name: '@cratis/eslint-config/ignores', ignores },
    {
        name: '@cratis/eslint-config/base',
        files: ['**/*.ts', '**/*.tsx'],
        plugins: {
            '@typescript-eslint': tseslintPlugin,
            react,
            'no-null': noNull,
        },
        languageOptions: {
            globals: { ...globals.browser },
            parser: tsParser,
            sourceType: 'module',
        },
        settings: {
            react: { version: 'detect' },
        },
        rules: coreRules,
    },
];

export default base;
