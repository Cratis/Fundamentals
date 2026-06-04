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

`eslint` is a peer dependency; everything else (typescript-eslint, eslint-plugin-react, …) ships transitively.

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
import arc from '@cratis/arc.eslint';
import components from '@cratis/components.eslint';

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
