import { Linter } from 'eslint';
import { describe, expect, it } from 'vitest';
import { consumer, internal } from '../index.js';

const linter = new Linter();
const lint = (code, config) => linter.verify(code, config, { filename: 'sample.ts' });
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
