import cratis from '@cratis/eslint-config';

// Fundamentals lints its own source with the Cratis-internal preset: the shared house
// hygiene every Cratis consumer gets, plus the Cratis-authoring layer (MIT license
// header). The rule body lives in @cratis/eslint-config (this repo's own workspace) so
// Arc, Components, and Fundamentals stop each carrying a copy. Repo-specific overrides,
// if ever needed, append to this array.
export default [
    ...cratis.configs.internal,
];
