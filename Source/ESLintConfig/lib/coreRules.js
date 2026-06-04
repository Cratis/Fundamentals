import eslintJs from '@eslint/js';
import tseslint from 'typescript-eslint';

// Flatten the `rules` maps from one or more flat-config objects into a single map.
// typescript-eslint and @eslint/js ship their recommended sets as flat-config arrays;
// we merge them down so the whole house rule set can live on one config block.
export const collectRules = configOrArray => {
    const items = Array.isArray(configOrArray) ? configOrArray : [configOrArray];
    return items.reduce((accumulated, item) => (item && item.rules ? { ...accumulated, ...item.rules } : accumulated), {});
};

// The Cratis house TypeScript/React rule set, product-agnostic. This is the rule body
// the Cratis product repos historically copy-pasted into each repo's eslint.config.mjs.
// It deliberately does NOT include the MIT license-header rule — that is an internal
// authoring concern layered on by the `internal` preset, never inherited by consumers.
export const coreRules = {
    ...collectRules(eslintJs.configs.recommended),
    ...collectRules(tseslint.configs.recommended),

    'no-irregular-whitespace': 0,
    semi: [2, 'always'],
    'react/display-name': 0,
    'react/react-in-jsx-scope': 0,
    'no-prototype-builtins': 0,

    '@typescript-eslint/no-unused-vars': ['error', { ignoreRestSiblings: true }],
    '@typescript-eslint/no-explicit-any': 'error',
    '@typescript-eslint/explicit-module-boundary-types': 0,
    '@typescript-eslint/no-non-null-assertion': 0,
    '@typescript-eslint/no-empty-function': 'error',
    '@typescript-eslint/no-var-requires': 'error',
    '@typescript-eslint/ban-ts-comment': 0,
    '@typescript-eslint/no-empty-interface': 0,
};
