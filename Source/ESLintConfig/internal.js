import header from '@tony.ganchev/eslint-plugin-header';
import base from './base.js';
import specs from './specs.js';

// The Cratis MIT license header, enforced on every Cratis-authored `.ts`/`.tsx`.
// This is an internal authoring rule: it belongs in the Cratis product repos
// (Arc, Components, Fundamentals, …), and consumers must NOT inherit it.
const licenseHeader = {
    name: '@cratis/eslint-config/license-header',
    files: ['**/*.ts', '**/*.tsx'],
    plugins: { '@tony.ganchev': header },
    rules: {
        '@tony.ganchev/header': ['error', {
            header: {
                commentType: 'line',
                lines: [
                    ' Copyright (c) Cratis. All rights reserved.',
                    ' Licensed under the MIT license. See LICENSE file in the project root for full license information.',
                ],
            },
        }],
    },
};

// What the Cratis product repos lint their own source with: the consumer preset plus
// the Cratis-authoring layer.
//
//   import cratis from '@cratis/eslint-config';
//   export default cratis.configs.internal;
const internal = [...base, ...specs, licenseHeader];

export default internal;
