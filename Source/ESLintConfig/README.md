# @cratis/eslint-config

Shared [ESLint flat-config](https://eslint.org/docs/latest/use/configure/configuration-files) presets for Cratis projects.

Two audiences, deliberately separated:

| Preset | For | Adds |
|---|---|---|
| `consumer` | Projects that **build on** Cratis (other products, apps) | House TypeScript/React hygiene + `for_*` BDD spec relaxations |
| `internal` | The Cratis **product repos** (Arc, Components, Fundamentals, …) | Everything in `consumer` **plus** the Cratis MIT license-header rule |

The license header is a Cratis *authoring* concern, so it lives only in `internal` — consumers never inherit it.

## Install

```sh
yarn add -D @cratis/eslint-config eslint
```

`eslint` and the TypeScript programmatic API are peer dependencies. The parser, TypeScript rules, and other plugins used by these presets ship transitively.

### TypeScript 7

TypeScript 7.0 supplies the native `tsc` compiler but not the JavaScript compiler API required by typescript-eslint. Use [TypeScript's supported side-by-side setup](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0/#running-side-by-side-with-typescript-6.0):

```sh
npm install -D @cratis/eslint-config eslint@latest \
  '@typescript/native@npm:typescript@latest' \
  'typescript@npm:@typescript/typescript6@latest'
```

The build still runs TypeScript 7's `tsc`; the separate compatibility package supplies the API used by lint tools and names its older compiler executable `tsc6`.

### React rules

JSX syntax and references are handled by the TypeScript parser and rules. These presets do not enable any `react/*` rules, and no longer install or register `eslint-plugin-react` implicitly. This avoids its incompatible ESLint 10 peer dependency.

If a custom configuration enables additional React-specific rules, register a compatible plugin explicitly in that configuration. The legacy `react/display-name` and `react/react-in-jsx-scope` entries remain disabled, and `settings.react.version` remains `detect`, so consumers that supply their own React plugin keep those defaults.

## Use — a project built on Cratis

```js
// eslint.config.mjs
import cratis from '@cratis/eslint-config';

export default [
    ...cratis.configs.consumer,
    // …your project-specific rules layered on top
];
```

If you consume Cratis Arc proxies or Cratis Components, also compose the product rule packages:

```js
import cratis from '@cratis/eslint-config';
import arc from '@cratis/eslint-plugin-arc';
import components from '@cratis/eslint-plugin-components';

export default [
    ...cratis.configs.consumer,
    ...arc.configs.recommended,         // skips generated proxies, MVVM view-model discipline
    ...components.configs.recommended,  // no raw primereact dialogs, subpath-only imports
    // …your project rules
];
```

## Use — inside a Cratis product repo

```js
// eslint.config.mjs
import cratis from '@cratis/eslint-config';

export default cratis.configs.internal;
```

## Building blocks

`configs.base` (hygiene + ignores) and `configs.specs` (`for_*` relaxations) are exported too, so you can compose your own preset. Named exports `base`, `specs`, `consumer`, `internal`, and `ignores` are also available.

## Verify the published peer graph

From the repository root, run:

```sh
node Source/ESLintConfig/scripts/verify-peer-graph.mjs
```

The check packs this workspace and installs the tarball into an isolated consumer under `.ai-work/`, using the latest ESLint and TypeScript/compiler-API packages. It requires strict peer resolution and a clean complete dependency tree, then verifies the packed presets against JSX and representative consumer/internal rules. CI runs this in addition to the preset specs; workspace hoisting must not hide a broken published dependency graph.
