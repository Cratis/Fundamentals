// Cratis BDD specifications live in `for_*` folders and read as scenario sentences.
// They intentionally relax a handful of rules: expression-statement assertions, free
// naming for `when_…`/`and_…` describers, and empty setup bodies.
const specs = [
    {
        files: ['**/for_*/**/*.ts', '**/for_*/**/*.tsx'],
        rules: {
            '@typescript-eslint/naming-convention': 0,
            '@typescript-eslint/no-unused-expressions': 0,
            '@typescript-eslint/no-empty-function': 'off',
            'no-restricted-globals': 0,
        },
    },
];

export default specs;
