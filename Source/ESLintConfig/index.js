import base, { ignores } from './base.js';
import consumer from './consumer.js';
import internal from './internal.js';
import specs from './specs.js';

// Composable Cratis ESLint flat-config presets.
//
//   base      product-agnostic TypeScript/React hygiene + default ignores
//   specs     `for_*` BDD spec relaxations
//   consumer  base + specs — for projects built ON Cratis (no license header)
//   internal  consumer + the Cratis MIT license header — for Cratis product repos
export const configs = { base, specs, consumer, internal };

export { base, specs, consumer, internal, ignores };

export default { configs };
