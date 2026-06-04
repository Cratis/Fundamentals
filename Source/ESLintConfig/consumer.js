import base from './base.js';
import specs from './specs.js';

// What a project consuming Cratis packages should lint with: the Cratis house
// TypeScript/React hygiene plus the BDD spec relaxations — WITHOUT the Cratis-internal
// MIT license header. Spread it first in your eslint.config, then layer project rules:
//
//   import cratis from '@cratis/eslint-config';
//   export default [...cratis.configs.consumer, ...yourProjectConfig];
const consumer = [...base, ...specs];

export default consumer;
