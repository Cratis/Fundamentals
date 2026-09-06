// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Linter } from 'eslint';
import { describe, expect, it } from 'vitest';
import { consumer, internal } from '../index.js';

const linter = new Linter();
const lint = (code, config, filename = 'sample.ts') => linter.verify(code, config, { filename });
const ruleIds = messages => messages.map(message => message.ruleId);

const headeredSource = [
    '// Copyright (c) Cratis. All rights reserved.',
    '// Licensed under the MIT license. See LICENSE file in the project root for full license information.',
    '',
    'export const value = 1;',
    '',
].join('\n');

describe('consumer preset', () => {
    it('flags no-explicit-any', () => {
        expect(ruleIds(lint('export const value: any = 1;\n', consumer))).toContain('@typescript-eslint/no-explicit-any');
    });

    it('requires semicolons', () => {
        expect(ruleIds(lint('export const value = 1\n', consumer))).toContain('semi');
    });

    it('does NOT enforce the Cratis license header', () => {
        expect(ruleIds(lint('export const value = 1;\n', consumer))).not.toContain('@tony.ganchev/header');
    });

    it('accepts components referenced only from JSX without a React plugin', () => {
        const source = "import { Button } from './Button';\nexport const View = () => <Button />;\n";
        expect(lint(source, consumer, 'sample.tsx')).toEqual([]);
    });

    it('still reports genuinely unused imports in JSX files', () => {
        const source = "import { Button } from './Button';\nexport const View = () => <div />;\n";
        expect(ruleIds(lint(source, consumer, 'sample.tsx'))).toContain('@typescript-eslint/no-unused-vars');
    });

    it('preserves version detection for a consumer-provided React plugin', () => {
        const suppliedReact = {
            plugins: {
                react: {
                    rules: {
                        'check-settings': {
                            meta: { schema: [] },
                            create: context => {
                                expect(context.settings.react).toEqual({ version: 'detect' });
                                return {};
                            },
                        },
                    },
                },
            },
            rules: { 'react/check-settings': 'error' },
        };
        expect(lint('export const View = () => <div />;\n', [...consumer, suppliedReact], 'sample.tsx')).toEqual([]);
    });

    it('keeps the optional React rules disabled when a consumer supplies that plugin', () => {
        const report = {
            meta: { schema: [] },
            create: context => ({ Program: node => context.report({ node, message: 'React rule ran' }) }),
        };
        const suppliedReact = {
            plugins: { react: { rules: { 'display-name': report, 'react-in-jsx-scope': report } } },
            rules: { 'react/display-name': 'error', 'react/react-in-jsx-scope': 'error' },
        };
        expect(lint('export const View = () => <div />;\n', [suppliedReact, ...consumer], 'sample.tsx')).toEqual([]);
    });
});

describe('internal preset', () => {
    it('shares the consumer hygiene rules', () => {
        expect(ruleIds(lint('export const value: any = 1;\n', internal))).toContain('@typescript-eslint/no-explicit-any');
    });

    it('enforces the Cratis license header on un-headered files', () => {
        expect(ruleIds(lint('export const value = 1;\n', internal))).toContain('@tony.ganchev/header');
    });

    it('accepts a correctly-headered file', () => {
        expect(ruleIds(lint(headeredSource, internal))).not.toContain('@tony.ganchev/header');
    });
});
